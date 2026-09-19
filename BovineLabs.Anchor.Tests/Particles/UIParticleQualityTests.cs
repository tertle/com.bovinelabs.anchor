namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class UIParticleQualityTests
    {
        private UIParticleEffect effect;

        [SetUp]
        public void SetUp()
        {
            this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            var emitter = this.effect.Emitters[0];
            emitter.Lifetime = new Vector2(100, 100);
            emitter.Speed = Vector2.zero;
            emitter.Duration = 0.03125f;
            emitter.Looping = true;
            emitter.Bursts[0] = new UIParticleBurst
            {
                Count = 1,
            };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(this.effect);

        [Test]
        public void FractionalBurstCreditIsDeterministicAcrossSteps()
        {
            using var a = new UIParticleRuntime(this.effect);
            using var b = new UIParticleRuntime(this.effect);
            a.EmissionScale = b.EmissionScale = 0.25f;
            a.Play(7);
            b.Play(7);
            a.Advance(0);
            b.Advance(0);
            for (var i = 0; i < 12; i++)
            {
                a.Advance(0.03125);
                b.Advance(0.015625);
                b.Advance(0.015625);
            }

            Assert.That(a.LiveCount, Is.EqualTo(3));
            Assert.That(b.LiveCount, Is.EqualTo(3));
            for (var i = 0; i < 3; i++)
            {
                Assert.That(a.Particles[i].Age, Is.EqualTo(b.Particles[i].Age).Within(0.00001));
                Assert.That(a.Particles[i].Lifetime, Is.EqualTo(100));
            }
        }

        [Test]
        public void ScaleChangesDoNotReplaySuppressedContinuousOrBurstEmission()
        {
            this.effect.Emitters[0].Rate = 128;
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.EmissionScale = 0;
            runtime.Play();
            runtime.Advance(0.0625);
            Assert.That(runtime.LiveCount, Is.Zero);
            runtime.EmissionScale = 1;
            runtime.Advance(0);
            Assert.That(runtime.LiveCount, Is.Zero);
            runtime.Advance(0.03125);
            Assert.That(runtime.LiveCount, Is.EqualTo(5));
            Assert.That(runtime.Counters.Attempted, Is.EqualTo(5));
            runtime.EmissionScale = 0;
            runtime.Advance(0.03125);
            Assert.That(runtime.LiveCount, Is.EqualTo(5));
            Assert.That(runtime.Particles[0].Age, Is.GreaterThan(0));
        }

        [Test]
        public void ScaledOverloadIsBoundedAndCountedWithoutDebt()
        {
            this.effect.Emitters[0].MaxParticles = 4;
            this.effect.Emitters[0].Bursts[0] = new UIParticleBurst
            {
                Count = 16,
            };
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.EmissionScale = 0.5f;
            runtime.Play();
            runtime.Advance(0);
            Assert.That(runtime.LiveCount, Is.EqualTo(4));
            Assert.That(runtime.Counters.Attempted, Is.EqualTo(8));
            Assert.That(runtime.Counters.Emitted, Is.EqualTo(4));
            Assert.That(runtime.Counters.Dropped, Is.EqualTo(4));
            runtime.Advance(0);
            Assert.That(runtime.Counters.Attempted, Is.EqualTo(8));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidScaleIsRejected(float scale)
        {
            using var runtime = new UIParticleRuntime(this.effect);
            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.EmissionScale = scale);
        }
    }
}
