namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Mathematics;
    using UnityEngine;

    public class UIParticleFlipbookTests
    {
        [TestCase(0.2499f, true, 0, 0)]
        [TestCase(0.25f, true, 1, 0)]
        [TestCase(1, true, 0, 1)]
        [TestCase(2, true, 0, 0)]
        [TestCase(2, false, 3, 1)]
        public void LifetimeFramesAdvanceAcrossRowsAndRespectLooping(float age, bool loop, int column, int row)
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 2,
                Loop = loop,
            };
            var region = new float4(0, 0, 1, 1);
            var compiled = UIParticleFlipbookUtility.Compile(settings, region, new int2(256, 128), 0);
            var uv = UIParticleFlipbookUtility.FrameUv(in compiled, int2.zero, age, 2, region);
            Assert.That(uv.x, Is.EqualTo(column / 4f).Within(0.00001f));
            Assert.That(uv.y, Is.EqualTo((1 - row) / 2f).Within(0.00001f));
            Assert.That(uv.z - uv.x, Is.EqualTo(0.25f).Within(0.00001f));
            Assert.That(uv.w - uv.y, Is.EqualTo(0.5f).Within(0.00001f));
        }

        [TestCase(0.1249f, 2)]
        [TestCase(0.125f, 3)]
        [TestCase(0.25f, 0)]
        public void FrameRateUsesAgeAndBirthFrameWithinTheSelectedRow(float age, int column)
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 3,
                Mode = UIParticleFlipbookMode.SingleRow,
                Row = 1,
                Timing = UIParticleFlipbookTiming.FramesPerSecond,
                FramesPerSecond = 8,
                StartFrame = new Vector2Int(2, 2),
            };
            var region = new float4(0.1f, 0.2f, 0.9f, 0.8f);
            var compiled = UIParticleFlipbookUtility.Compile(settings, region, new int2(256), 0);
            var birth = UIParticleFlipbookUtility.Sample(in compiled, 7, 0, 0);
            var uv = UIParticleFlipbookUtility.FrameUv(in compiled, birth, age, 100, region);
            Assert.That(uv.x, Is.EqualTo(0.1f + (column * 0.2f)).Within(0.00001f));
            Assert.That(uv.y, Is.EqualTo(0.4f).Within(0.00001f));
            Assert.That(uv.w, Is.EqualTo(0.6f).Within(0.00001f));
        }

        [Test]
        public void FlippingPreservesFrameSelectionAndInsetsEachCell()
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 2,
                Rows = 2,
                Padding = 2,
            };
            var region = new float4(0.9f, 0.8f, 0.1f, 0.2f);
            var compiled = UIParticleFlipbookUtility.Compile(settings, region, new int2(100, 200), 0);
            var uv = UIParticleFlipbookUtility.FrameUv(in compiled, int2.zero, 0, 1, region);
            Assert.That(uv.x, Is.EqualTo(0.48f).Within(0.00001f));
            Assert.That(uv.z, Is.EqualTo(0.12f).Within(0.00001f));
            Assert.That(uv.y, Is.EqualTo(0.79f).Within(0.00001f));
            Assert.That(uv.w, Is.EqualTo(0.51f).Within(0.00001f));
        }

        [Test]
        public void RandomRowsAndStartFramesReplayWithoutSharingTheBirthRandomStream()
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 8,
                Rows = 4,
                Mode = UIParticleFlipbookMode.SingleRow,
                RandomRow = true,
                StartFrame = new Vector2Int(2, 6),
            };
            var compiled = UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0);
            var frames = new HashSet<int>();
            var rows = new HashSet<int>();
            for (uint ordinal = 0; ordinal < 64; ordinal++)
            {
                var sample = UIParticleFlipbookUtility.Sample(in compiled, 17, 3, ordinal);
                Assert.That(sample, Is.EqualTo(UIParticleFlipbookUtility.Sample(in compiled, 17, 3, ordinal)));
                Assert.That(sample.x, Is.InRange(2, 6));
                Assert.That(sample.y, Is.InRange(0, 3));
                frames.Add(sample.x);
                rows.Add(sample.y);
            }

            Assert.That(frames.Count, Is.GreaterThan(1));
            Assert.That(rows.Count, Is.GreaterThan(1));
        }

        [TestCase(0)]
        [TestCase(float.MaxValue)]
        public void FiniteExtremeRatesKeepFramesInsideTheGrid(float rate)
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 2,
                Timing = UIParticleFlipbookTiming.FramesPerSecond,
                FramesPerSecond = rate,
            };
            var region = new float4(0, 0, 1, 1);
            var compiled = UIParticleFlipbookUtility.Compile(settings, region, new int2(256), 0);
            var uv = UIParticleFlipbookUtility.FrameUv(in compiled, new int2(2, 0), float.MaxValue, float.MaxValue, region);
            Assert.That(math.all(math.isfinite(uv)), Is.True);
            Assert.That(math.all(uv >= 0) && math.all(uv <= 1), Is.True);
            if (rate == 0)
            {
                Assert.That(uv.x, Is.EqualTo(0.5f));
            }
        }

        [TestCase(0, 1)]
        [TestCase(int.MaxValue, 2)]
        public void InvalidGridDimensionsAreRejected(int columns, int rows)
        {
            var settings = new UIParticleFlipbookSettings {
                Columns = columns,
                Rows = rows
            };
            Assert.Throws<ArgumentException>(() => UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0));
        }

        [TestCase(-1)]
        [TestCase(float.NaN)]
        public void InvalidSelectedTimingIsRejected(float rate)
        {
            var settings = new UIParticleFlipbookSettings {
                Cycles = rate
            };
            Assert.Throws<ArgumentException>(() => UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0));
        }

        [TestCase(-1)]
        [TestCase(float.NaN)]
        [TestCase(32)]
        public void InvalidOrFrameConsumingPaddingIsRejected(float padding)
        {
            var settings = new UIParticleFlipbookSettings {
                Columns = 4,
                Padding = padding
            };
            Assert.Throws<ArgumentException>(() => UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0));
        }

        [Test]
        public void SingleRowFrameRangesCannotAddressOtherRows()
        {
            var settings = new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 2,
                Mode = UIParticleFlipbookMode.SingleRow,
                StartFrame = new Vector2Int(0, 4),
            };
            Assert.Throws<ArgumentException>(() => UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0));
            settings.StartFrame = new Vector2Int(2, 1);
            Assert.Throws<ArgumentException>(() => UIParticleFlipbookUtility.Compile(settings, new float4(0, 0, 1, 1), new int2(256), 0));
        }
    }
}
