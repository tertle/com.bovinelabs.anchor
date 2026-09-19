namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class UIParticlePerformanceTests
    {
        private bool previousBurst;
        private bool previousSynchronous;

        [SetUp]
        public void SetUp()
        {
            this.previousBurst = BurstCompiler.Options.EnableBurstCompilation;
            this.previousSynchronous = BurstCompiler.Options.EnableBurstCompileSynchronously;
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
        }

        [TearDown]
        public void TearDown()
        {
            BurstCompiler.Options.EnableBurstCompileSynchronously = this.previousSynchronous;
            BurstCompiler.Options.EnableBurstCompilation = this.previousBurst;
        }

        [TestCase(64, 1)]
        [TestCase(256, 1)]
        [TestCase(1000, 1)]
        [TestCase(5000, 1)]
        [TestCase(20000, 1)]
        [TestCase(3200, 1)]
        [TestCase(3200, 32)]
        [TestCase(3200, 100)]
        [Performance]
        public void RuntimeMatrix(int total, int controls)
        {
            Assert.That(BurstCompiler.IsEnabled, Is.True);
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            var count = total / controls;
            var settings = effect.Emitters[0];
            settings.MaxParticles = count;
            settings.Bursts[0] = new UIParticleBurst
            {
                Count = count,
            };
            settings.Lifetime = new Vector2(100, 100);
            var runtimes = new UIParticleRuntime[controls];
            using var vertices = new NativeArray<Vertex>(count * 4, Allocator.Persistent);
            using var indices = new NativeArray<ushort>(count * 6, Allocator.Persistent);
            try
            {
                for (var i = 0; i < controls; i++)
                {
                    runtimes[i] = new UIParticleRuntime(effect);
                }

                void Reset()
                {
                    foreach (var runtime in runtimes)
                    {
                        runtime.Play(7);
                        runtime.Advance(0);
                        runtime.PrepareMesh(new float4(1));
                    }
                }

                void Simulate()
                {
                    foreach (var runtime in runtimes)
                    {
                        runtime.Advance(1d / 60);
                    }
                }

                void Mesh()
                {
                    foreach (var runtime in runtimes)
                    {
                        runtime.PrepareMesh(new float4(1));
                        for (var offset = 0; offset < count;)
                        {
                            var chunk = ParticleQuadMesh.GetChunkSize(count - offset);
                            var source = runtime.Quads.Slice(offset, chunk);
                            var output = vertices.Slice(offset * 4, chunk * 4);
                            var triangles = indices.Slice(offset * 6, chunk * 6);
                            ParticleQuadMesh.Fill(ref source, ref output, ref triangles);
                            offset += chunk;
                        }
                    }
                }

                void Combined()
                {
                    Simulate();
                    Mesh();
                }

                void PrepareDeaths()
                {
                    Reset();
                    foreach (var runtime in runtimes)
                    {
                        runtime.StopEmitting();
                        for (var p = 0; p < count; p += 2)
                        {
                            var particle = runtime.Particles[p];
                            particle.Lifetime = 0.001f;
                            runtime.Particles[p] = particle;
                        }
                    }
                }

                Reset();
                Combined();
                Measure.Method(Simulate).SetUp(Reset).SampleGroup(new SampleGroup("SimulationBatch", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                Measure.Method(Mesh).SetUp(Reset).SampleGroup(new SampleGroup("AppearanceAndMeshBatch", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                Measure.Method(Combined).SetUp(Reset).SampleGroup(new SampleGroup("CombinedBatch", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                Measure.Method(Simulate).SetUp(PrepareDeaths).SampleGroup(new SampleGroup("SimulationWithHalfDeathsBatch", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                Assert.That(runtimes[0].LiveCount, Is.EqualTo(count / 2));
                Measure.Custom(new SampleGroup("PreparedLiveParticles", SampleUnit.Undefined), total);
                Measure.Custom(new SampleGroup("ControlsPerBatch", SampleUnit.Undefined), controls);
                Measure.Custom(new SampleGroup("ParticleStride", SampleUnit.Byte), UIParticleRuntime.ParticleStride);
                Measure.Custom(new SampleGroup("ParticleCapacityBytes", SampleUnit.Byte), runtimes[0].ParticleCapacityBytes * controls);
                Measure.Custom(new SampleGroup("InstanceNativeBytes", SampleUnit.Byte), runtimes[0].NativeBytes * controls);
                Measure.Custom(new SampleGroup("SharedCompiledBytes", SampleUnit.Byte), runtimes[0].SharedCompiledBytes);
            }
            finally
            {
                foreach (var runtime in runtimes)
                {
                    runtime?.Dispose();
                }

                Object.DestroyImmediate(effect);
            }
        }
    }
}
