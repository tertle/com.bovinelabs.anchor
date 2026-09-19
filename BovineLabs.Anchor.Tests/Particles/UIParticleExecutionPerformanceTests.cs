namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class UIParticleExecutionPerformanceTests
    {
        [TestCase(64, 1)]
        [TestCase(1000, 1)]
        [TestCase(1000, 32)]
        [TestCase(1000, 100)]
        [TestCase(5000, 1)]
        [TestCase(20000, 1)]
        [Performance]
        public void DirectVersusScheduledStableCompaction(int total, int emitters)
        {
            var previousBurst = BurstCompiler.Options.EnableBurstCompilation;
            var previousSync = BurstCompiler.Options.EnableBurstCompileSynchronously;
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            effect.Emitters.Clear();
            for (var e = 0; e < emitters; e++)
            {
                var count = (total / emitters) + (e < total % emitters ? 1 : 0);
                effect.Emitters.Add(new UIParticleEmitterSettings
                {
                    MaxParticles = count,
                    Lifetime = new Vector2(100, 100),
                    Bursts = new()
                    {
                        new UIParticleBurst
                        {
                            Count = count,
                        },
                    },
                });
            }

            try
            {
                using var runtime = new UIParticleRuntime(effect);
                using var streams = new NativeArray<UIParticleEmissionStream>(emitters * 2, Allocator.Persistent);
                using var expected = new NativeArray<UIParticle>(total, Allocator.Persistent);
                var job = new StepJob
                {
                    Data = runtime.Compiled.Data,
                    Particles = runtime.Particles,
                    States = runtime.States,
                    Streams = streams,
                    Transform = float4x4.identity,
                };
                void Reset()
                {
                    runtime.Play(7);
                    runtime.Advance(0);
                    for (var e = 0; e < emitters; e++)
                    {
                        var emitter = runtime.Compiled.Data.Emitters[e];
                        for (var p = 0; p < emitter.Capacity; p += 2)
                        {
                            var particle = runtime.Particles[emitter.Offset + p];
                            particle.Lifetime = 0.001f;
                            runtime.Particles[emitter.Offset + p] = particle;
                        }
                    }
                }

                void Direct() => job.Execute();
                void Scheduled() => job.Schedule().Complete();
                Reset();
                Direct();
                expected.CopyFrom(runtime.Particles);
                Reset();
                Scheduled();
                for (var e = 0; e < emitters; e++)
                {
                    var emitter = runtime.Compiled.Data.Emitters[e];
                    Assert.That(runtime.States[e].Count, Is.EqualTo(emitter.Capacity / 2));
                    for (var p = 0; p < runtime.States[e].Count; p++)
                    {
                        var index = emitter.Offset + p;
                        Assert.That(runtime.Particles[index].Position, Is.EqualTo(expected[index].Position));
                        Assert.That(runtime.Particles[index].Tint, Is.EqualTo(expected[index].Tint));
                        Assert.That(runtime.Particles[index].Age, Is.EqualTo(expected[index].Age));
                    }
                }

                for (var repeat = 0; repeat < 5; repeat++)
                {
                    Measure.Method(Direct).SetUp(Reset).SampleGroup(new SampleGroup("Direct" + repeat, SampleUnit.Microsecond))
                        .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                    Measure.Method(Scheduled).SetUp(Reset).SampleGroup(new SampleGroup("ScheduleAndComplete" + repeat, SampleUnit.Microsecond))
                        .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                }
            }
            finally
            {
                Object.DestroyImmediate(effect);
                BurstCompiler.Options.EnableBurstCompileSynchronously = previousSync;
                BurstCompiler.Options.EnableBurstCompilation = previousBurst;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct StepJob : IJob
        {
            public UIParticleCompiledData Data;
            public NativeArray<UIParticle> Particles;
            public NativeArray<UIParticleEmitterState> States;
            public NativeArray<UIParticleEmissionStream> Streams;
            public float4x4 Transform;

            public void Execute()
            {
                UIParticleSimulation.Step(ref this.Data, ref this.Particles, ref this.States, ref this.Streams, 1d / 60, 7, false, ref this.Transform);
            }
        }
    }
}
