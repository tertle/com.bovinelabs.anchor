namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Mathematics;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class UIParticleCompilationTests
    {
        private UIParticleEffect effect;

        [SetUp]
        public void SetUp() => this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(this.effect);

        [Test]
        public void SharedRevisionsStayImmutableUntilTheirLastOwnerReleases()
        {
            var a = new UIParticleRuntime(this.effect);
            var b = new UIParticleRuntime(this.effect);
            try
            {
                Assert.That(a.Compiled, Is.SameAs(b.Compiled));
                this.effect.Emitters[0].MaxParticles = 256;
                this.effect.Invalidate();
                using var c = new UIParticleRuntime(this.effect);
                Assert.That(c.Compiled, Is.Not.SameAs(a.Compiled));
                Assert.That(a.Capacity, Is.EqualTo(128));
                Assert.That(c.Capacity, Is.EqualTo(256));
                a.Dispose();
                Assert.That(b.Compiled.IsDisposed, Is.False);
                b.Play();
                b.Advance(0);
                Assert.That(b.LiveCount, Is.EqualTo(16));
                b.Dispose();
                Assert.That(b.Compiled.IsDisposed, Is.True);
                Assert.That(c.Compiled.IsDisposed, Is.False);
            }
            finally
            {
                a.Dispose();
                b.Dispose();
            }
        }

        [Test]
        public void SampledCurveAndGradientIncludeBothEndpoints()
        {
            var s = this.effect.Emitters[0];
            s.SizeOverLifetime = AnimationCurve.Linear(0, 2, 1, 4);
            s.ColorOverLifetime.SetKeys(
                new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.blue, 1) },
                new[] { new GradientAlphaKey(0.25f, 0), new GradientAlphaKey(0.75f, 1) });
            using var runtime = new UIParticleRuntime(this.effect);
            var data = runtime.Compiled.Data;
            UIParticleMesh.Sample(ref data, 0, 0, out var size, out var color);
            Assert.That(size, Is.EqualTo(2));
            Assert.That(color, Is.EqualTo(new float4(1, 0, 0, 0.25f)));
            UIParticleMesh.Sample(ref data, 0, 1, out size, out color);
            Assert.That(size, Is.EqualTo(4));
            Assert.That(color, Is.EqualTo(new float4(0, 0, 1, 0.75f)));
            UIParticleMesh.Sample(ref data, 0, 0.5f, out size, out color);
            Assert.That(size, Is.EqualTo(3).Within(0.00001));
        }

        [Test]
        public void FixedGradientRetainsUnalignedColorAndAlphaTransitions()
        {
            var gradient = this.effect.Emitters[0].ColorOverLifetime;
            gradient.mode = GradientMode.Fixed;
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.green, 0.371f), new GradientColorKey(Color.blue, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0.4f, 0.619f), new GradientAlphaKey(0, 1) });
            using var runtime = new UIParticleRuntime(this.effect);
            var data = runtime.Compiled.Data;
            foreach (var age in new[] { 0f, 0.0001f, 0.3709f, 0.371f, 0.3711f, 0.6189f, 0.619f, 0.6191f, 0.9999f, 1f })
            {
                UIParticleMesh.Sample(ref data, 0, age, out _, out var color);
                var expected = gradient.Evaluate(age);
                Assert.That(color, Is.EqualTo(new float4(expected.r, expected.g, expected.b, expected.a)), $"age {age}");
            }
        }

        [TestCase("duration")]
        [TestCase("delay")]
        [TestCase("rate")]
        [TestCase("capacity")]
        [TestCase("shape")]
        [TestCase("offset")]
        [TestCase("dimensions")]
        [TestCase("spread")]
        [TestCase("lifetime")]
        [TestCase("speed")]
        [TestCase("size")]
        [TestCase("rotation")]
        [TestCase("spin")]
        [TestCase("acceleration")]
        [TestCase("drag")]
        [TestCase("color")]
        [TestCase("uv")]
        [TestCase("burst")]
        [TestCase("curve")]
        public void InvalidConfigurationIsRejectedBeforeNativeAllocation(string field)
        {
            var s = this.effect.Emitters[0];
            switch (field)
            {
                case "duration": s.Duration = 0; break;
                case "delay": s.StartDelay = -1; break;
                case "rate": s.Rate = float.PositiveInfinity; break;
                case "capacity": s.MaxParticles = 0; break;
                case "shape": s.Shape = (UIParticleShape)99; break;
                case "offset": s.Offset = new Vector2(float.NaN, 0); break;
                case "dimensions": s.Dimensions = new Vector2(-1, 0); break;
                case "spread": s.Spread = 361; break;
                case "lifetime": s.Lifetime = Vector2.zero; break;
                case "speed": s.Speed = new Vector2(5, 1); break;
                case "size": s.Size = new Vector2(-1, 0); break;
                case "rotation": s.Rotation = new Vector2(0, float.NaN); break;
                case "spin": s.AngularVelocity = new Vector2(0, float.PositiveInfinity); break;
                case "acceleration": s.Acceleration = new Vector2(float.NaN, 0); break;
                case "drag": s.Drag = -1; break;
                case "color": s.StartColorMax = Color.black; break;
                case "uv": s.Uv = new Rect(0.5f, 0, 1, 1); break;
                case "burst":
                    s.Bursts[0] = new UIParticleBurst
                    {
                        Time = 1,
                        Count = 1,
                    };
                    break;
                case "curve": s.SizeOverLifetime = AnimationCurve.Constant(0, 1, -1); break;
            }

            Assert.Throws<ArgumentException>(() => new UIParticleRuntime(this.effect));
        }

        [Test]
        public void CapacitySumOverflowIsRejectedBeforeNativeAllocation()
        {
            this.effect.Emitters[0].MaxParticles = int.MaxValue;
            this.effect.Emitters.Add(new UIParticleEmitterSettings());
            Assert.Throws<OverflowException>(() => new UIParticleRuntime(this.effect));
        }

        [TestCase(-1d)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void InvalidElapsedTimeIsRejected(double elapsed)
        {
            using var runtime = new UIParticleRuntime(this.effect);
            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Advance(elapsed));
        }
    }
}
