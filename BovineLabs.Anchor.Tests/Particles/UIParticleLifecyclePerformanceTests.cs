namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.PerformanceTesting;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class UIParticleLifecyclePerformanceTests
    {
        [TestCase(64)]
        [TestCase(1000)]
        [TestCase(20000)]
        [Performance]
        public void PreparationReplayAndRelease(int capacity)
        {
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            effect.Emitters[0].MaxParticles = capacity;
            effect.Emitters[0].Bursts[0] = new UIParticleBurst
            {
                Count = capacity,
            };
            var oldBurst = BurstCompiler.Options.EnableBurstCompilation;
            var oldSync = BurstCompiler.Options.EnableBurstCompileSynchronously;
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
            try
            {
                for (var i = 0; i < 30; i++)
                {
                    UIParticleRuntime prepared;
                    using (Measure.Scope(new SampleGroup("PrepareIncludingTables", SampleUnit.Microsecond)))
                    {
                        prepared = new UIParticleRuntime(effect);
                    }

                    using (Measure.Scope(new SampleGroup("ReleaseLastOwner", SampleUnit.Microsecond)))
                    {
                        prepared.Dispose();
                    }
                }

                using var runtime = new UIParticleRuntime(effect);
                runtime.Play(7);
                runtime.Advance(0);
                Measure.Method(() =>
                    {
                        runtime.Play(7);
                        runtime.Advance(0);
                    }).SampleGroup(new SampleGroup("ReplayPrepared", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                Measure.Custom(new SampleGroup("ResidentInstanceBytes", SampleUnit.Byte), runtime.NativeBytes);
                Measure.Custom(new SampleGroup("SharedCompiledBytes", SampleUnit.Byte), runtime.SharedCompiledBytes);
                Assert.That(runtime.LiveCount, Is.EqualTo(capacity));
            }
            finally
            {
                Object.DestroyImmediate(effect);
                BurstCompiler.Options.EnableBurstCompileSynchronously = oldSync;
                BurstCompiler.Options.EnableBurstCompilation = oldBurst;
            }
        }
    }
}
