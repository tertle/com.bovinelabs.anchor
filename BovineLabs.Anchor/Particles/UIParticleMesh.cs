namespace BovineLabs.Anchor.Particles
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;

    [BurstCompile]
    internal static class UIParticleMesh
    {
        [BurstCompile]
        internal static void Fill(
            ref UIParticleCompiledData data, ref NativeArray<UIParticle> particles, ref NativeArray<UIParticleEmitterState> states,
            ref NativeArray<ParticleQuad> quads, ref NativeArray<int> counts, ref float4 tint)
        {
            var output = 0;
            for (var e = 0; e < data.Emitters.Length; e++)
            {
                var emitter = data.Emitters[e];
                var count = 0;
                for (var p = 0; p < states[e].Count; p++)
                {
                    var particle = particles[emitter.Offset + p];
                    var age = math.saturate(particle.Age / particle.Lifetime);
                    Sample(ref data, e, age, out var multiplier, out var color);
                    var size = particle.Size * multiplier;
                    if (size == 0)
                    {
                        continue;
                    }

                    color *= particle.Tint * tint;
                    var bytes = (uint4)math.round(math.saturate(color) * 255);
                    quads[output++] = new ParticleQuad
                    {
                        Position = particle.Position,
                        Size = new float2(size),
                        Angle = particle.Rotation,
                        Tint = new Color32((byte)bytes.x, (byte)bytes.y, (byte)bytes.z, (byte)bytes.w),
                        Uv = emitter.Uv,
                    };
                    count++;
                }

                counts[e] = count;
            }
        }

        internal static void Sample(ref UIParticleCompiledData data, int emitterIndex, float age, out float size, out float4 color)
        {
            var t = math.saturate(age) * (UIParticleCompiledEffect.SampleCount - 1);
            var index = (int)t;
            var offset = emitterIndex * UIParticleCompiledEffect.SampleCount;
            var a = data.Samples[offset + index];
            var b = data.Samples[offset + math.min(index + 1, UIParticleCompiledEffect.SampleCount - 1)];
            size = math.lerp(a.Size, b.Size, t - index);
            color = math.lerp(a.Color, b.Color, t - index);
            var emitter = data.Emitters[emitterIndex];
            if (emitter.StepCount == 0)
            {
                return;
            }

            var low = 0;
            var high = emitter.StepCount;
            while (low + 1 < high)
            {
                var mid = (low + high) / 2;
                if (data.Steps[emitter.StepOffset + mid].Time <= age)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }

            var step = data.Steps[emitter.StepOffset + low];
            color = age == step.Time ? step.At : step.After;
        }
    }
}
