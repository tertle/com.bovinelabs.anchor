namespace BovineLabs.Anchor.Particles
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;

    [BurstCompile]
    internal static class UIParticleSimulation
    {
        [BurstCompile]
        internal static void Step(
            ref UIParticleCompiledData data, ref NativeArray<UIParticle> particles, ref NativeArray<UIParticleEmitterState> states,
            ref NativeArray<UIParticleEmissionStream> streams, double delta, uint seed, bool emitting, ref float4x4 spawnTransform)
        {
            for (var e = 0; e < data.Emitters.Length; e++)
            {
                var settings = data.Emitters[e];
                var state = states[e];
                Coefficients(settings.Drag, (float)delta, out var decay, out var velocityIntegral, out var accelerationIntegral);
                var live = 0;
                for (var p = 0; p < state.Count; p++)
                {
                    var particle = particles[settings.Offset + p];
                    Integrate(ref particle, (float)delta, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
                    if (particle.Age < particle.Lifetime)
                    {
                        particles[settings.Offset + live++] = particle;
                    }
                }

                state.Count = live;
                var end = state.Time + delta;
                if (emitting && end >= settings.Delay)
                {
                    var from = math.max(0, state.Time - settings.Delay);
                    var to = math.max(0, end - settings.Delay);
                    if (!settings.Looping)
                    {
                        from = math.min(from, settings.Duration);
                        to = math.min(to, settings.Duration);
                    }

                    var first = !state.Started;
                    state.Started = true;
                    var streamOffset = settings.BurstOffset + e;
                    var continuousBefore = math.floor(from * settings.Rate);
                    var continuousAfter = math.floor(to * settings.Rate);
                    var attempted = continuousAfter - continuousBefore;
                    streams[streamOffset] = new UIParticleEmissionStream
                    {
                        Remaining = attempted,
                        Time = settings.Rate > 0 ? (continuousBefore + 1) / settings.Rate : double.PositiveInfinity,
                        Cycle = continuousBefore + 1,
                    };
                    for (var b = 0; b < settings.BurstCount; b++)
                    {
                        var burst = data.Bursts[settings.BurstOffset + b];
                        var before = first ? 0 : Occurrences(from, burst.Time, settings.Duration, settings.Looping);
                        var after = Occurrences(to, burst.Time, settings.Duration, settings.Looping);
                        var count = (after - before) * burst.Count;
                        attempted += count;
                        streams[streamOffset + b + 1] = new UIParticleEmissionStream
                        {
                            Remaining = count,
                            Time = (before * settings.Duration) + burst.Time,
                            Cycle = before,
                        };
                    }

                    // Admission is fixed after compaction. Newborn deaths do not allow unlimited retries in this substep.
                    var admitted = (int)math.min(settings.Capacity - live, attempted);
                    for (var p = 0; p < admitted; p++)
                    {
                        var selected = -1;
                        var birth = double.PositiveInfinity;
                        for (var s = 0; s <= settings.BurstCount; s++)
                        {
                            var candidate = streams[streamOffset + s];
                            if (candidate.Remaining > 0 && candidate.Time < birth)
                            {
                                selected = s;
                                birth = candidate.Time;
                            }
                        }

                        var stream = streams[streamOffset + selected];
                        stream.Remaining--;
                        if (selected == 0)
                        {
                            stream.Cycle++;
                            stream.Time = stream.Cycle / settings.Rate;
                        }
                        else
                        {
                            var burst = data.Bursts[settings.BurstOffset + selected - 1];
                            if (++stream.InBurst == burst.Count)
                            {
                                stream.InBurst = 0;
                                stream.Cycle++;
                                stream.Time = (stream.Cycle * settings.Duration) + burst.Time;
                            }
                        }

                        streams[streamOffset + selected] = stream;
                        var particle = Spawn(in settings, seed, (uint)e, state.Ordinal + (uint)p, ref spawnTransform);
                        var age = (float)math.max(0, end - settings.Delay - birth);
                        Coefficients(settings.Drag, age, out decay, out velocityIntegral, out accelerationIntegral);
                        Integrate(ref particle, age, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
                        if (particle.Age < particle.Lifetime)
                        {
                            particles[settings.Offset + state.Count++] = particle;
                        }
                    }

                    state.Attempted = AddCounter(state.Attempted, attempted);
                    state.Emitted = AddCounter(state.Emitted, admitted);
                    state.Dropped = AddCounter(state.Dropped, attempted - admitted);
                    state.Ordinal = unchecked(state.Ordinal + (uint)(attempted % 4294967296d));
                }

                state.Time = end;
                states[e] = state;
            }
        }

        private static double Occurrences(double time, double burstTime, double duration, bool looping)
        {
            if (time < burstTime)
            {
                return 0;
            }

            return looping ? math.floor((time - burstTime) / duration) + 1 : 1;
        }

        private static ulong AddCounter(ulong value, double increment)
        {
            return increment >= ulong.MaxValue - value ? ulong.MaxValue : value + (ulong)increment;
        }

        private static UIParticle Spawn(in UIParticleEmitter settings, uint seed, uint emitter, uint ordinal, ref float4x4 transform)
        {
            var random = new Random(math.hash(new uint3(seed, emitter, ordinal)) | 1u);
            var position = settings.Position;
            switch (settings.Shape)
            {
                case UIParticleShape.Line:
                    position.x += (random.NextFloat() - 0.5f) * settings.Dimensions.x;
                    break;
                case UIParticleShape.Rectangle:
                    position += (random.NextFloat2() - 0.5f) * settings.Dimensions;
                    break;
                case UIParticleShape.Circle:
                    math.sincos(random.NextFloat() * (2 * math.PI), out var sin, out var cos);
                    position += new float2(cos, sin) * (math.sqrt(random.NextFloat()) * settings.Dimensions.x * 0.5f);
                    break;
            }

            var direction = settings.Direction + ((random.NextFloat() - 0.5f) * settings.Spread);
            math.sincos(direction, out var dy, out var dx);
            var velocity = new float2(dx, dy) * random.NextFloat(settings.Speed.x, settings.Speed.y);
            return new UIParticle
            {
                Position = math.transform(transform, new float3(position, 0)).xy,
                Velocity = math.mul(transform, new float4(velocity, 0, 0)).xy,
                Lifetime = random.NextFloat(settings.Lifetime.x, settings.Lifetime.y),
                Size = random.NextFloat(settings.Size.x, settings.Size.y),
                Rotation = random.NextFloat(settings.Rotation.x, settings.Rotation.y),
                AngularVelocity = random.NextFloat(settings.AngularVelocity.x, settings.AngularVelocity.y),
                Tint = math.lerp(settings.ColorMin, settings.ColorMax, random.NextFloat4()),
                Basis = new float2x2(transform.c0.xy, transform.c1.xy),
            };
        }

        // Exact constant-acceleration linear-drag solution; the small-x series avoids cancellation near zero drag.
        private static void Coefficients(float drag, float delta, out float decay, out float velocityIntegral, out float accelerationIntegral)
        {
            var x = drag * delta;
            decay = math.exp(-x);
            if (x < 0.01f)
            {
                velocityIntegral = delta * (1 - (x * 0.5f) + (x * x / 6) - (x * x * x / 24));
                accelerationIntegral = delta * delta * (0.5f - (x / 6) + (x * x / 24) - (x * x * x / 120));
            }
            else
            {
                velocityIntegral = (1 - decay) / drag;
                accelerationIntegral = (delta - velocityIntegral) / drag;
            }
        }

        private static void Integrate(
            ref UIParticle particle, float delta, float2 acceleration, float decay, float velocityIntegral, float accelerationIntegral)
        {
            particle.Age += delta;
            particle.Position += (particle.Velocity * velocityIntegral) + (acceleration * accelerationIntegral);
            particle.Velocity = (particle.Velocity * decay) + (acceleration * velocityIntegral);
            particle.Rotation += particle.AngularVelocity * delta;
        }
    }
}
