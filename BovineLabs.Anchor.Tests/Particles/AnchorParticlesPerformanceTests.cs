namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using System.Collections;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.PerformanceTesting;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class AnchorParticlesPerformanceTests
    {
        [UnityTest, Performance]
        public IEnumerator NormalContentAndHiddenWatchers()
        {
            var window = ScriptableObject.CreateInstance<EditorWindow>();
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            try
            {
                effect.Emitters.Clear();
                for (var i = 0; i < 32; i++)
                {
                    var count = i < 8 ? 32 : 31;
                    effect.Emitters.Add(new UIParticleEmitterSettings
                    {
                        MaxParticles = count,
                        Lifetime = new Vector2(1000, 1000),
                        Bursts = new()
                        {
                            new UIParticleBurst
                            {
                                Count = count,
                            },
                        },
                    });
                }

                window.Show();
                var parent = new VisualElement();
                window.rootVisualElement.Add(parent);
                var control = new AnchorParticles
                {
                    Effect = effect,
                    PlayOnAttach = false,
                };
                control.style.width = 300;
                control.style.height = 300;
                parent.Add(control);
                yield return null;
                yield return null;
                control.Play();
                for (var i = 0; i < 20; i++)
                {
                    control.Advance(1d / 60);
                }

                Assert.That(control.LiveCount, Is.EqualTo(1000));
                Measure.Method(() => control.Advance(1d / 60)).SampleGroup(new SampleGroup("Control1000Particles32Emitters", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
                var before = GC.GetAllocatedBytesForCurrentThread();
                for (var i = 0; i < 100; i++)
                {
                    control.Advance(1d / 60);
                }

                var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allocated, Is.Zero);
                Measure.Custom(new SampleGroup("ControlAdvanceManagedBytes", SampleUnit.Byte), allocated);
                parent.style.display = DisplayStyle.None;
                yield return null;
                control.Advance(0);
                Measure.Method(() => control.Advance(1d / 60)).SampleGroup(new SampleGroup("HiddenPauseWatch", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(100).GC().Run();
                control.Pause();
                Measure.Method(() => control.Advance(1d / 60)).SampleGroup(new SampleGroup("ExplicitPause", SampleUnit.Microsecond))
                    .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(100).GC().Run();
                control.RemoveFromHierarchy();
                Assert.That(control.LiveCount, Is.Zero);
            }
            finally
            {
                window.Close();
                Object.DestroyImmediate(effect);
            }
        }
    }
}
