namespace BovineLabs.Anchor.Particles
{
    using System;
    using Unity.Mathematics;
    using Random = Unity.Mathematics.Random;

    internal static class UIParticleFlightUtility
    {
        internal static UIParticleFlight Compile(UIParticleFlightSettings settings, int offset)
        {
            var scatter = (float2)settings.ScatterDuration;
            var duration = (float2)settings.FlightDuration;
            if (!math.all(math.isfinite(scatter)) || scatter.x < 0 || scatter.y < scatter.x ||
                !math.all(math.isfinite(duration)) || duration.x <= 0 || duration.y < duration.x || !math.isfinite(scatter.y + duration.y))
            {
                throw new ArgumentException("Flight durations must be finite ordered ranges: scatter is nonnegative, flight is positive, and their sum is finite.");
            }

            if (!math.isfinite(settings.PathBend) || settings.PathBend < 0 || !math.isfinite(settings.ArrivalRadius) || settings.ArrivalRadius < 0)
            {
                throw new ArgumentException("Flight bend and arrival radius must be finite and nonnegative.");
            }

            if (settings.TargetMode < UIParticleTargetMode.Snapshot || settings.TargetMode > UIParticleTargetMode.Follow)
            {
                throw new ArgumentException("Unknown particle target mode.");
            }

            return new UIParticleFlight
            {
                Enabled = true,
                Offset = offset,
                ScatterDuration = scatter,
                FlightDuration = duration,
                PathBend = settings.PathBend,
                ArrivalRadius = settings.ArrivalRadius,
                TargetMode = settings.TargetMode,
            };
        }

        internal static UIParticleFlightState Sample(in UIParticleFlight settings, uint seed, uint emitter, uint ordinal)
        {
            var random = new Random(math.hash(new uint4(seed, emitter, ordinal, 0x517cc1b7u)) | 1u);
            return new UIParticleFlightState
            {
                ScatterDuration = random.NextFloat(settings.ScatterDuration.x, settings.ScatterDuration.y),
                FlightDuration = random.NextFloat(settings.FlightDuration.x, settings.FlightDuration.y),
                Bend = random.NextFloat(-settings.PathBend, settings.PathBend),
            };
        }

        internal static void Begin(ref UIParticleFlightState state, float2 start, float2 target)
        {
            var direction = target - start;
            var bend = math.normalizesafe(new float2(-direction.y, direction.x)) * state.Bend;
            state.Start = start;
            state.Control1 = start + (direction / 3) + bend;
            state.Control2 = start + (direction * (2f / 3)) + bend;
            state.Destination = target;
            state.Started = true;
        }

        internal static float2 Evaluate(in UIParticleFlightState state, UIParticleTargetMode mode, float2 target, float time)
        {
            var t = math.saturate(time);
            var end = mode == UIParticleTargetMode.Follow ? target : state.Destination;
            var control2 = state.Control2 + (end - state.Destination);
            // De Casteljau avoids large cubic coefficients and handles coincident endpoints without a direction normalization.
            var a = math.lerp(state.Start, state.Control1, t);
            var b = math.lerp(state.Control1, control2, t);
            var c = math.lerp(control2, end, t);
            return math.lerp(math.lerp(a, b, t), math.lerp(b, c, t), t);
        }
    }
}
