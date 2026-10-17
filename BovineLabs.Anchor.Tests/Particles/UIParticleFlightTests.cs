namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Mathematics;
    using UnityEngine;

    public class UIParticleFlightTests
    {
        [TestCase(-1, 1), TestCase(0, 0), TestCase(0, float.PositiveInfinity)]
        public void InvalidDurationRangesAreRejected(float scatter, float duration)
        {
            var settings = new UIParticleFlightSettings
            {
                ScatterDuration = new Vector2(scatter, scatter),
                FlightDuration = new Vector2(duration, duration),
            };
            Assert.Throws<ArgumentException>(() => UIParticleFlightUtility.Compile(settings, 0));
        }

        [Test]
        public void StraightPathClampsTimeAndCoincidentEndpointsStayFinite()
        {
            var state = new UIParticleFlightState();
            UIParticleFlightUtility.Begin(ref state, new float2(2, 4), new float2(14, 4));
            Assert.That(UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Snapshot, default, -1), Is.EqualTo(new float2(2, 4)));
            Assert.That(UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Snapshot, default, 0.25f), Is.EqualTo(new float2(5, 4)));
            Assert.That(UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Snapshot, default, 2), Is.EqualTo(new float2(14, 4)));
            state.Bend = 50;
            UIParticleFlightUtility.Begin(ref state, new float2(7, 9), new float2(7, 9));
            Assert.That(UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Follow, new float2(7, 9), 0.5f), Is.EqualTo(new float2(7, 9)));
        }

        [Test]
        public void FollowMovesTheEndpointSideOfTheCurveWhileSnapshotKeepsItsDestination()
        {
            var state = new UIParticleFlightState {
                Bend = 12
            };
            UIParticleFlightUtility.Begin(ref state, float2.zero, new float2(60, 0));
            var original = UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Snapshot, new float2(100, 40), 0.5f);
            var follow = UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Follow, new float2(100, 40), 0.5f);
            Assert.That(original, Is.EqualTo(new float2(30, 9)));
            Assert.That(follow, Is.EqualTo(original + new float2(20, 20)));
            Assert.That(UIParticleFlightUtility.Evaluate(in state, UIParticleTargetMode.Follow, new float2(100, 40), 1), Is.EqualTo(new float2(100, 40)));
            Assert.That(state.Destination, Is.EqualTo(new float2(60, 0)));
        }

        [Test]
        public void FlightSamplingIsRepeatableAndSeparatesBirthOrdinals()
        {
            var settings = UIParticleFlightUtility.Compile(new UIParticleFlightSettings(), 0);
            var first = UIParticleFlightUtility.Sample(in settings, 17, 2, 9);
            var replay = UIParticleFlightUtility.Sample(in settings, 17, 2, 9);
            var next = UIParticleFlightUtility.Sample(in settings, 17, 2, 10);
            Assert.That(first, Is.EqualTo(replay));
            Assert.That(next, Is.Not.EqualTo(first));
            Assert.That(first.ScatterDuration, Is.InRange(settings.ScatterDuration.x, settings.ScatterDuration.y));
            Assert.That(first.FlightDuration, Is.InRange(settings.FlightDuration.x, settings.FlightDuration.y));
            Assert.That(first.Bend, Is.InRange(-settings.PathBend, settings.PathBend));
        }
    }
}
