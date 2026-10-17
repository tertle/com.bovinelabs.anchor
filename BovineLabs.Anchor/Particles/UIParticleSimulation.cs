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
            ref NativeArray<UIParticleEmissionStream> streams, ref NativeArray<int2> flipbookStates, ref UIParticleFlights flights, double delta, uint seed,
            bool emitting, ref float4x4 spawnTransform, float emissionScale = 1)
        {
            for (var e = 0; e < data.Emitters.Length; e++)
            {
                var settings = data.Emitters[e];
                var state = states[e];
                var flipbook = data.Flipbooks.IsCreated ? data.Flipbooks[e] : default;
                var flight = data.Flights.IsCreated ? data.Flights[e] : default;
                Coefficients(settings.Drag, (float)delta, out var decay, out var velocityIntegral, out var accelerationIntegral);
                var live = 0;
                if (flight.Enabled)
                {
                    for (var p = 0; p < state.Count; p++)
                    {
                        var particle = particles[settings.Offset + p];
                        var phase = flights.States[flight.Offset + p];
                        if (AdvanceFlight(ref particle, ref phase, in settings, in flight, flights.Target, (float)delta))
                        {
                            flights.States[flight.Offset + live] = phase;
                            if (flipbook.Enabled)
                            {
                                flipbookStates[flipbook.Offset + live] = flipbookStates[flipbook.Offset + p];
                            }

                            particles[settings.Offset + live++] = particle;
                        }
                        else
                        {
                            flights.Arrivals++;
                        }
                    }
                }
                else if (flipbook.Enabled)
                {
                    for (var p = 0; p < state.Count; p++)
                    {
                        var particle = particles[settings.Offset + p];
                        Integrate(ref particle, (float)delta, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
                        if (particle.Age < particle.Lifetime)
                        {
                            flipbookStates[flipbook.Offset + live] = flipbookStates[flipbook.Offset + p];
                            particles[settings.Offset + live++] = particle;
                        }
                    }
                }
                else
                {
                    for (var p = 0; p < state.Count; p++)
                    {
                        var particle = particles[settings.Offset + p];
                        Integrate(ref particle, (float)delta, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
                        if (particle.Age < particle.Lifetime)
                        {
                            particles[settings.Offset + live++] = particle;
                        }
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
                    var rate = (double)settings.Rate * emissionScale;
                    var previousCredit = streams[streamOffset].Credit;
                    var credit = previousCredit + ((to - from) * rate);
                    var attempted = math.floor(credit);
                    streams[streamOffset] = new UIParticleEmissionStream
                    {
                        Remaining = attempted,
                        Time = rate > 0 ? from + ((1 - previousCredit) / rate) : double.PositiveInfinity,
                        Credit = credit - attempted,
                    };
                    for (var b = 0; b < settings.BurstCount; b++)
                    {
                        var burst = data.Bursts[settings.BurstOffset + b];
                        var before = first ? 0 : Occurrences(from, burst.Time, settings.Duration, settings.Looping);
                        var after = Occurrences(to, burst.Time, settings.Duration, settings.Looping);
                        var scaledCount = (double)burst.Count * emissionScale;
                        var initialCredit = streams[streamOffset + b + 1].Credit;
                        var burstCredit = initialCredit + ((after - before) * scaledCount);
                        var count = math.floor(burstCredit);
                        var firstCycle = scaledCount > 0 ? math.max(0, math.ceil((1 - initialCredit) / scaledCount) - 1) : 0;
                        attempted += count;
                        streams[streamOffset + b + 1] = new UIParticleEmissionStream
                        {
                            Remaining = count,
                            Time = ((before + firstCycle) * settings.Duration) + burst.Time,
                            Cycle = before,
                            Credit = burstCredit - count,
                            InitialCredit = initialCredit,
                            ScaledCount = scaledCount,
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
                            stream.Time += 1 / rate;
                        }
                        else
                        {
                            var burst = data.Bursts[settings.BurstOffset + selected - 1];
                            stream.InBurst++;
                            // Skip suppressed occurrences analytically; work stays bounded by admitted capacity even at tiny scales.
                            var cycle = math.max(0, math.ceil(((double)stream.InBurst + 1 - stream.InitialCredit) / stream.ScaledCount) - 1);
                            stream.Time = ((stream.Cycle + cycle) * settings.Duration) + burst.Time;
                        }

                        streams[streamOffset + selected] = stream;
                        var particle = Spawn(in settings, in flight, seed, (uint)e, state.Ordinal + (uint)p, ref spawnTransform, out var phase);
                        var age = (float)math.max(0, end - settings.Delay - birth);
                        var survives = true;
                        if (flight.Enabled)
                        {
                            survives = AdvanceFlight(ref particle, ref phase, in settings, in flight, flights.Target, age);
                            if (!survives)
                            {
                                flights.Arrivals++;
                            }
                        }
                        else
                        {
                            Coefficients(settings.Drag, age, out decay, out velocityIntegral, out accelerationIntegral);
                            Integrate(ref particle, age, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
                            survives = particle.Age < particle.Lifetime;
                        }

                        if (survives)
                        {
                            if (flight.Enabled)
                            {
                                flights.States[flight.Offset + state.Count] = phase;
                            }

                            if (flipbook.Enabled)
                            {
                                flipbookStates[flipbook.Offset + state.Count] = UIParticleFlipbookUtility.Sample(in flipbook, seed, (uint)e,
                                    unchecked(state.Ordinal + (uint)p));
                            }

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

        [BurstCompile]
        internal static void Emit(
            ref UIParticleCompiledData data, ref NativeArray<UIParticle> particles, ref NativeArray<UIParticleEmitterState> states,
            ref NativeArray<int2> flipbookStates, ref UIParticleFlights flights, int emitterIndex, int count, uint seed, float emissionScale,
            ref float4x4 spawnTransform, out ParticleEmissionResult result)
        {
            var settings = data.Emitters[emitterIndex];
            var state = states[emitterIndex];
            var flipbook = data.Flipbooks.IsCreated ? data.Flipbooks[emitterIndex] : default;
            var flight = data.Flights.IsCreated ? data.Flights[emitterIndex] : default;
            var credit = state.ManualCredit + ((double)count * emissionScale);
            var attempted = math.floor(credit);
            state.ManualCredit = credit - attempted;
            var admitted = (int)math.min(settings.Capacity - state.Count, attempted);
            for (var p = 0; p < admitted; p++)
            {
                var ordinal = unchecked(state.Ordinal + (uint)p);
                var particle = Spawn(in settings, in flight, seed, (uint)emitterIndex, ordinal, ref spawnTransform, out var phase);
                if (flight.Enabled)
                {
                    flights.States[flight.Offset + state.Count] = phase;
                }

                if (flipbook.Enabled)
                {
                    flipbookStates[flipbook.Offset + state.Count] = UIParticleFlipbookUtility.Sample(in flipbook, seed, (uint)emitterIndex, ordinal);
                }

                particles[settings.Offset + state.Count++] = particle;
            }

            state.Attempted = AddCounter(state.Attempted, attempted);
            state.Emitted = AddCounter(state.Emitted, admitted);
            state.Dropped = AddCounter(state.Dropped, attempted - admitted);
            state.Ordinal = unchecked(state.Ordinal + (uint)(attempted % 4294967296d));
            states[emitterIndex] = state;
            result = new ParticleEmissionResult
            {
                Status = ParticleEmissionStatus.Accepted,
                Attempted = AddCounter(0, attempted),
                Emitted = admitted,
                Dropped = AddCounter(0, attempted - admitted),
            };
        }

        private static bool AdvanceFlight(
            ref UIParticle particle, ref UIParticleFlightState phase, in UIParticleEmitter settings, in UIParticleFlight flight, float2 target, float delta)
        {
            var scatter = math.min(delta, math.max(0, phase.ScatterDuration - particle.Age));
            if (scatter > 0)
            {
                Coefficients(settings.Drag, scatter, out var decay, out var velocityIntegral, out var accelerationIntegral);
                Integrate(ref particle, scatter, settings.Acceleration, decay, velocityIntegral, accelerationIntegral);
            }

            particle.Age += delta - scatter;
            particle.Rotation += particle.AngularVelocity * (delta - scatter);
            if (particle.Age < phase.ScatterDuration)
            {
                return true;
            }

            if (!phase.Started)
            {
                UIParticleFlightUtility.Begin(ref phase, particle.Position, target);
            }

            var endpoint = flight.TargetMode == UIParticleTargetMode.Follow ? target : phase.Destination;
            if (particle.Age >= particle.Lifetime)
            {
                particle.Position = endpoint;
                return false;
            }

            particle.Position = UIParticleFlightUtility.Evaluate(in phase, flight.TargetMode, target,
                (particle.Age - phase.ScatterDuration) / phase.FlightDuration);
            return math.distance(particle.Position, endpoint) > flight.ArrivalRadius;
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

        private static UIParticle Spawn(
            in UIParticleEmitter settings, in UIParticleFlight flight, uint seed, uint emitter, uint ordinal, ref float4x4 transform,
            out UIParticleFlightState phase)
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
            var lifetime = random.NextFloat(settings.Lifetime.x, settings.Lifetime.y);
            phase = default;
            if (flight.Enabled)
            {
                phase = UIParticleFlightUtility.Sample(in flight, seed, emitter, ordinal);
                lifetime = phase.ScatterDuration + phase.FlightDuration;
            }

            return new UIParticle
            {
                Position = math.transform(transform, new float3(position, 0)).xy,
                Velocity = math.mul(transform, new float4(velocity, 0, 0)).xy,
                Lifetime = lifetime,
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
