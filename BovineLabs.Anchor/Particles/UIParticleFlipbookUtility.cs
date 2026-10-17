namespace BovineLabs.Anchor.Particles
{
    using System;
    using Unity.Mathematics;
    using Random = Unity.Mathematics.Random;

    internal static class UIParticleFlipbookUtility
    {
        internal static UIParticleFlipbook Compile(UIParticleFlipbookSettings settings, float4 region, int2 textureSize, int offset)
        {
            if (settings.Columns <= 0 || settings.Rows <= 0 || (long)settings.Columns * settings.Rows > int.MaxValue)
            {
                throw new ArgumentException("Flipbook rows and columns must be positive, with at most Int32.MaxValue frames.");
            }

            if (settings.Mode > UIParticleFlipbookMode.SingleRow || settings.Timing > UIParticleFlipbookTiming.FramesPerSecond)
            {
                throw new ArgumentException("Unknown flipbook mode or timing.");
            }

            var count = settings.Mode == UIParticleFlipbookMode.SingleRow ? settings.Columns : settings.Columns * settings.Rows;
            if (settings.StartFrame.x < 0 || settings.StartFrame.y < settings.StartFrame.x || settings.StartFrame.y >= count)
            {
                throw new ArgumentException("Flipbook start frames must be an ordered inclusive range inside the selected sheet or row.");
            }

            if (settings.Mode == UIParticleFlipbookMode.SingleRow && !settings.RandomRow && (settings.Row < 0 || settings.Row >= settings.Rows))
            {
                throw new ArgumentException("Flipbook row must be inside the grid; row zero is the top row.");
            }

            var rate = settings.Timing == UIParticleFlipbookTiming.Lifetime ? settings.Cycles : settings.FramesPerSecond;
            if (!math.isfinite(rate) || rate < 0)
            {
                throw new ArgumentException("Flipbook cycles or frame rate must be finite and nonnegative.");
            }

            var cellPixels = math.abs(region.zw - region.xy) * textureSize / new float2(settings.Columns, settings.Rows);
            if (!math.isfinite(settings.Padding) || settings.Padding < 0 || math.any(cellPixels <= settings.Padding * 2))
            {
                throw new ArgumentException("Flipbook padding must be finite, nonnegative and leave a nonempty frame.");
            }

            return new UIParticleFlipbook
            {
                Enabled = true,
                Offset = offset,
                Columns = settings.Columns,
                Rows = settings.Rows,
                FrameCount = count,
                Mode = settings.Mode,
                RandomRow = settings.RandomRow,
                Row = settings.Row,
                StartFrame = new int2(settings.StartFrame.x, settings.StartFrame.y),
                Timing = settings.Timing,
                Rate = rate,
                Loop = settings.Loop,
                Padding = settings.Padding / (float2)textureSize,
            };
        }

        internal static int2 Sample(in UIParticleFlipbook settings, uint seed, uint emitter, uint ordinal)
        {
            // A separate stream preserves every existing birth parameter when animation is enabled.
            var random = new Random(math.hash(new uint4(seed, emitter, ordinal, 0x9e3779b9u)) | 1u);
            var frame = random.NextInt(settings.StartFrame.x, settings.StartFrame.y + 1);
            var row = settings.RandomRow ? random.NextInt(settings.Rows) : settings.Row;
            return new int2(frame, row);
        }

        internal static float4 FrameUv(in UIParticleFlipbook settings, int2 birth, float age, float lifetime, float4 region)
        {
            // Double intermediates also keep finite large rates/ages from overflowing before wrapping.
            var elapsed = settings.Timing == UIParticleFlipbookTiming.Lifetime
                ? math.saturate((double)age / lifetime) * settings.Rate * settings.FrameCount
                : (double)age * settings.Rate;
            var frame = math.floor(birth.x + elapsed);
            var index = (int)(settings.Loop ? frame % settings.FrameCount : math.min(frame, settings.FrameCount - 1));
            var row = settings.Mode == UIParticleFlipbookMode.SingleRow ? birth.y : index / settings.Columns;
            var cell = math.abs(region.zw - region.xy) / new float2(settings.Columns, settings.Rows);
            var min = math.min(region.xy, region.zw) + (new float2(index % settings.Columns, settings.Rows - row - 1) * cell);
            var lower = min + settings.Padding;
            var upper = min + cell - settings.Padding;
            var flipped = region.zw < region.xy;
            return new float4(math.select(lower, upper, flipped), math.select(upper, lower, flipped));
        }
    }
}
