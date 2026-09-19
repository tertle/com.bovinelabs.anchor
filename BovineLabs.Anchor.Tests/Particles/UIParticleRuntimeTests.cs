namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [BurstCompile]
    public class UIParticleRuntimeTests
    {
        private UIParticleEffect effect;
        private bool previousBurst;
        private bool previousSynchronous;

        [SetUp]
        public void SetUp()
        {
            this.previousBurst = BurstCompiler.Options.EnableBurstCompilation;
            this.previousSynchronous = BurstCompiler.Options.EnableBurstCompileSynchronously;
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
            this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            var s = this.effect.Emitters[0];
            s.Lifetime = new Vector2(1, 1);
            s.Speed = Vector2.zero;
            s.Size = new Vector2(2, 2);
            s.SizeOverLifetime = AnimationCurve.Constant(0, 1, 1);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this.effect);
            BurstCompiler.Options.EnableBurstCompileSynchronously = this.previousSynchronous;
            BurstCompiler.Options.EnableBurstCompilation = this.previousBurst;
        }

        [Test]
        public void ZeroDeltaEmitsInitialBurstOnceAndRestartResetsIt()
        {
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0);
            runtime.Advance(0);
            Assert.That(runtime.LiveCount, Is.EqualTo(16));
            Assert.That(runtime.States[0].Attempted, Is.EqualTo(16));
            Assert.That(runtime.Particles[0].Age, Is.Zero);
            runtime.Play();
            Assert.That(runtime.LiveCount, Is.Zero);
            runtime.Advance(0);
            Assert.That(runtime.LiveCount, Is.EqualTo(16));
            Assert.That(runtime.States[0].Attempted, Is.EqualTo(16));
        }

        [Test]
        public void InitialDelayIsConsumedOnlyOnceAcrossLoops()
        {
            var s = this.effect.Emitters[0];
            s.Duration = 0.03125f;
            s.StartDelay = 0.03125f;
            s.Looping = true;
            s.Bursts[0] = new UIParticleBurst
            {
                Count = 1,
            };
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0.015625);
            Assert.That(runtime.LiveCount, Is.Zero);
            runtime.Advance(0.015625);
            Assert.That(runtime.LiveCount, Is.EqualTo(1));
            runtime.Advance(0.0625);
            Assert.That(runtime.LiveCount, Is.EqualTo(3));
            runtime.Advance(0);
            Assert.That(runtime.LiveCount, Is.EqualTo(3));
        }

        [Test]
        public void ContinuousCreditAndBirthAgesAgreeAcrossOrdinaryPartitions()
        {
            var s = this.effect.Emitters[0];
            s.Bursts.Clear();
            s.Rate = 128;
            using var a = new UIParticleRuntime(this.effect);
            using var b = new UIParticleRuntime(this.effect);
            a.Play();
            b.Play();
            a.Advance(0.0625);
            for (var i = 0; i < 8; i++)
            {
                b.Advance(0.0078125);
            }

            Assert.That(a.LiveCount, Is.EqualTo(8));
            Assert.That(b.LiveCount, Is.EqualTo(8));
            for (var i = 0; i < 8; i++)
            {
                Assert.That(a.Particles[i].Age, Is.EqualTo((7 - i) / 128f).Within(0.000001));
                Assert.That(a.Particles[i].Age, Is.EqualTo(b.Particles[i].Age).Within(0.000001));
            }
        }

        [Test]
        public void SortedBurstsCrossLoopBoundaryOnceInBirthOrder()
        {
            var s = this.effect.Emitters[0];
            s.Duration = 0.03125f;
            s.Looping = true;
            s.Bursts.Clear();
            s.Bursts.Add(new UIParticleBurst
            {
                Time = 0.015625f,
                Count = 1,
            });
            s.Bursts.Add(new UIParticleBurst
            {
                Time = 0,
                Count = 1,
            });
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0.0625);
            Assert.That(runtime.LiveCount, Is.EqualTo(5));
            for (var i = 0; i < 5; i++)
            {
                Assert.That(runtime.Particles[i].Age, Is.EqualTo((4 - i) * 0.015625f).Within(0.000001));
            }

            Assert.That(s.Bursts[0].Time, Is.EqualTo(0.015625f), "Compilation must not reorder authored data.");
        }

        [Test]
        public void LargeElapsedTimeIsDiscardedWithoutDebt()
        {
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(100);
            Assert.That(runtime.Particles[0].Age, Is.EqualTo(4f / 60).Within(0.000001));
            runtime.Advance(0);
            Assert.That(runtime.Particles[0].Age, Is.EqualTo(4f / 60).Within(0.000001));
            runtime.Advance(1d / 240);
            Assert.That(runtime.Particles[0].Age, Is.EqualTo((4f / 60) + (1f / 240)).Within(0.000001));
        }

        [Test]
        public void PauseStopResumeClearAndCompletionRespectPlaybackContract()
        {
            var s = this.effect.Emitters[0];
            s.Lifetime = new Vector2(0.03125f, 0.03125f);
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0);
            runtime.Pause();
            Assert.That(runtime.Advance(0.05), Is.False);
            Assert.That(runtime.Particles[0].Age, Is.Zero);
            runtime.StopEmitting();
            runtime.Resume();
            runtime.Advance(0.03125);
            Assert.That(runtime.LiveCount, Is.Zero);
            Assert.That(runtime.IsPlaying, Is.False);
            runtime.Resume();
            Assert.That(runtime.IsPlaying, Is.False);
            runtime.Play();
            runtime.Advance(0);
            runtime.PrepareMesh(new float4(1));
            runtime.Clear();
            Assert.That(runtime.QuadCounts[0], Is.Zero);
            runtime.Resume();
            Assert.That(runtime.Advance(0), Is.False);
        }

        [Test]
        public void SeededReplayDoesNotTouchUnityRandom()
        {
            var s = this.effect.Emitters[0];
            s.Shape = UIParticleShape.Circle;
            s.Speed = new Vector2(1, 50);
            var saved = UnityEngine.Random.state;
            using var a = new UIParticleRuntime(this.effect);
            using var b = new UIParticleRuntime(this.effect);
            a.Play(19);
            b.Play(19);
            for (var i = 0; i < 10; i++)
            {
                a.Advance(0.01);
                b.Advance(0.01);
            }

            Assert.That(a.Particles.ToArray(), Is.EqualTo(b.Particles.ToArray()));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(saved));
        }

        [Test]
        public void SaturationDiscardsWholeCreditAndAdvancesSpawnOrdinals()
        {
            var s = this.effect.Emitters[0];
            s.Bursts.Clear();
            s.MaxParticles = 1;
            s.Rate = 256;
            s.Lifetime = new Vector2(0.015625f, 0.015625f);
            s.Speed = new Vector2(1, 20);
            using var small = new UIParticleRuntime(this.effect);
            s.MaxParticles = 16;
            this.effect.Invalidate();
            using var large = new UIParticleRuntime(this.effect);
            small.Play(42);
            large.Play(42);
            small.Advance(0.0078125);
            large.Advance(0.0078125);
            Assert.That(small.States[0].Attempted, Is.EqualTo(2));
            Assert.That(small.States[0].Dropped, Is.EqualTo(1));
            small.Advance(0.015625);
            large.Advance(0.015625);
            Assert.That(small.States[0].Attempted, Is.EqualTo(6));
            Assert.That(small.States[0].Emitted, Is.EqualTo(2));
            Assert.That(small.Particles[0].Velocity, Is.EqualTo(large.Particles[0].Velocity));
            Assert.That(small.Particles[0].Age, Is.EqualTo(large.Particles[0].Age));
            small.Advance(0);
            Assert.That(small.States[0].Attempted, Is.EqualTo(6));
        }

        [Test]
        public void ExtremeEmissionAndTinyLoopsDoBoundedWorkAndSaturateCounters()
        {
            var s = this.effect.Emitters[0];
            s.MaxParticles = 2;
            s.Duration = float.Epsilon;
            s.Looping = true;
            s.Rate = float.MaxValue;
            s.Bursts[0] = new UIParticleBurst
            {
                Count = int.MaxValue,
            };
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0.01);
            Assert.That(runtime.LiveCount, Is.EqualTo(2));
            Assert.That(runtime.States[0].Attempted, Is.EqualTo(ulong.MaxValue));
            Assert.That(runtime.States[0].Dropped, Is.EqualTo(ulong.MaxValue));
            Assert.That(runtime.States[0].Emitted, Is.EqualTo(2));
        }

        [TestCase(UIParticleShape.Point)]
        [TestCase(UIParticleShape.Line)]
        [TestCase(UIParticleShape.Rectangle)]
        [TestCase(UIParticleShape.Circle)]
        public void ShapesStayInsideAuthoredBounds(UIParticleShape shape)
        {
            var s = this.effect.Emitters[0];
            s.Shape = shape;
            s.Dimensions = new Vector2(10, 6);
            s.Offset = new Vector2(30, 40);
            s.MaxParticles = 4096;
            s.Bursts[0] = new UIParticleBurst
            {
                Count = 4096,
            };
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0);
            var radiusSquared = 0f;
            for (var p = 0; p < runtime.LiveCount; p++)
            {
                var position = runtime.Particles[p].Position - (float2)s.Offset;
                Assert.That(math.abs(position.x), Is.LessThanOrEqualTo(5));
                Assert.That(math.abs(position.y), Is.LessThanOrEqualTo(shape == UIParticleShape.Circle ? 5 : 3));
                if (shape == UIParticleShape.Point)
                {
                    Assert.That(position, Is.EqualTo(float2.zero));
                }

                if (shape == UIParticleShape.Line)
                {
                    Assert.That(position.y, Is.Zero);
                }

                if (shape == UIParticleShape.Circle)
                {
                    Assert.That(math.lengthsq(position), Is.LessThanOrEqualTo(25.00001));
                    radiusSquared += math.lengthsq(position);
                }
            }

            if (shape == UIParticleShape.Circle)
            {
                Assert.That(radiusSquared / runtime.LiveCount, Is.EqualTo(12.5).Within(0.5), "Uniform area has E[r²] = R²/2.");
            }
        }

        [TestCase(0f)]
        [TestCase(2f)]
        [TestCase(0.00001f)]
        public void MotionMatchesConstantAccelerationDragAndSpin(float drag)
        {
            var s = this.effect.Emitters[0];
            s.Speed = new Vector2(10, 10);
            s.Spread = 0;
            s.Acceleration = new Vector2(4, 0);
            s.Drag = drag;
            s.AngularVelocity = new Vector2(90, 90);
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0.0625);
            const double t = 0.0625;
            var integral = drag == 0 ? t : (1 - Math.Exp(-drag * t)) / drag;
            var acceleration = drag == 0 ? t * t / 2 : (t - integral) / drag;
            Assert.That(runtime.Particles[0].Position.x, Is.EqualTo((10 * integral) + (4 * acceleration)).Within(0.00001));
            Assert.That(runtime.Particles[0].Velocity.x, Is.EqualTo((10 * Math.Exp(-drag * t)) + (4 * integral)).Within(0.00001));
            Assert.That(runtime.Particles[0].Rotation, Is.EqualTo(math.PI * t / 2).Within(0.000001));
        }

        [Test]
        public void CompactionKeepsSurvivorsInBirthOrder()
        {
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0);
            for (var p = 0; p < runtime.LiveCount; p++)
            {
                var particle = runtime.Particles[p];
                particle.Position = new float2(p, 0);
                particle.Lifetime = p % 2 == 0 ? 0.005f : 1;
                runtime.Particles[p] = particle;
            }

            runtime.StopEmitting();
            runtime.Advance(0.01);
            Assert.That(runtime.LiveCount, Is.EqualTo(8));
            runtime.PrepareMesh(new float4(1));
            for (var p = 0; p < runtime.LiveCount; p++)
            {
                Assert.That(runtime.Particles[p].Position.x, Is.EqualTo((p * 2) + 1));
                Assert.That(runtime.Quads[p].Position.x, Is.EqualTo((p * 2) + 1));
            }
        }

        [Test]
        public void MultipleEmittersPreserveLayeringTintAndZeroSizeParticles()
        {
            this.effect.Emitters[0].Offset = new Vector2(10, 0);
            this.effect.Emitters[0].StartColorMin = this.effect.Emitters[0].StartColorMax = new Color(1, 1, 1, 0.5f);
            this.effect.Emitters.Add(new UIParticleEmitterSettings
            {
                Offset = new Vector2(20, 0),
                Size = new Vector2(2, 2),
                Speed = Vector2.zero,
            });
            this.effect.Emitters.Add(new UIParticleEmitterSettings
            {
                Size = Vector2.zero,
            });
            using var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            runtime.Advance(0);
            runtime.PrepareMesh(new float4(1, 1, 1, 0.5f));
            Assert.That(runtime.LiveCount, Is.EqualTo(48));
            Assert.That(runtime.QuadCounts.ToArray(), Is.EqualTo(new[] { 16, 16, 0 }));
            Assert.That(runtime.Quads[0].Position.x, Is.EqualTo(10));
            Assert.That(runtime.Quads[16].Position.x, Is.EqualTo(20));
            Assert.That(runtime.Quads[0].Tint.a, Is.EqualTo(64));
        }

        [Test]
        public void PanelBirthTransformSeparatesPointsFromVectors()
        {
            var s = this.effect.Emitters[0];
            s.Offset = new Vector2(3, 4);
            s.Speed = new Vector2(10, 10);
            s.Spread = 0;
            using var runtime = new UIParticleRuntime(this.effect);
            var transform = float4x4.TRS(new float3(100, 200, 0), quaternion.RotateZ(math.PI / 2), new float3(2, 2, 1));
            runtime.Play();
            runtime.Advance(0, ref transform);
            Assert.That(math.distance(runtime.Particles[0].Position, new float2(92, 206)), Is.LessThan(0.00001));
            Assert.That(math.distance(runtime.Particles[0].Velocity, new float2(0, 20)), Is.LessThan(0.00001));
        }

        [Test]
        public void EmptyEffectAndDisposedRuntimeHaveExplicitLifetimes()
        {
            this.effect.Emitters.Clear();
            var runtime = new UIParticleRuntime(this.effect);
            runtime.Play();
            Assert.That(runtime.Advance(1), Is.False);
            Assert.That(runtime.LiveCount, Is.Zero);
            runtime.Dispose();
            runtime.Dispose();
            Assert.Throws<ObjectDisposedException>(() => runtime.Play());
        }

        [Test]
        public void SimulationAndMeshActuallyExecuteInBurst()
        {
            using var runtime = new UIParticleRuntime(this.effect);
            var streams = new NativeArray<UIParticleEmissionStream>(2, Allocator.Temp);
            var data = runtime.Compiled.Data;
            BurstProbe(ref data, ref runtime.Particles, ref runtime.States, ref streams, ref runtime.Quads, ref runtime.QuadCounts, out var native);
            Assert.That(native, Is.True, "The probe must compile both complete production call paths into native code.");
            Assert.That(runtime.LiveCount, Is.EqualTo(16));
            Assert.That(runtime.QuadCounts[0], Is.EqualTo(16));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static void BurstProbe(
            ref UIParticleCompiledData data, ref NativeArray<UIParticle> particles, ref NativeArray<UIParticleEmitterState> states,
            ref NativeArray<UIParticleEmissionStream> streams, ref NativeArray<ParticleQuad> quads, ref NativeArray<int> counts, out bool native)
        {
            native = true;
            ManagedProbe(ref native);
            var identity = float4x4.identity;
            var tint = new float4(1);
            UIParticleSimulation.Step(ref data, ref particles, ref states, ref streams, 0, 1, true, ref identity);
            UIParticleMesh.Fill(ref data, ref particles, ref states, ref quads, ref counts, ref tint);
        }

        [BurstDiscard]
        private static void ManagedProbe(ref bool native) => native = false;
    }
}
