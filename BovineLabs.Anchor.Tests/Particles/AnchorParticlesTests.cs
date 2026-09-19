namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using System.Collections;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class AnchorParticlesTests
    {
        private EditorWindow window;
        private UIParticleEffect effect;
        private AnchorParticles particles;
        private VisualElement parent;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            this.window = ScriptableObject.CreateInstance<EditorWindow>();
            this.window.Show();
            this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            this.effect.Emitters[0].Lifetime = new Vector2(0.1f, 0.1f);
            this.parent = new VisualElement();
            this.window.rootVisualElement.Add(this.parent);
            this.particles = new AnchorParticles
            {
                Effect = this.effect,
                PlayOnAttach = false,
            };
            this.particles.style.width = 200;
            this.particles.style.height = 200;
            this.parent.Add(this.particles);
            yield return null;
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            this.window.Close();
            Object.DestroyImmediate(this.effect);
        }

        [Test]
        public void DetachReleasesLastNativeOwnerAndReattachStartsWithoutOldParticles()
        {
            using var owner = new UIParticleRuntime(this.effect);
            this.particles.Play();
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            owner.Dispose();
            Assert.That(owner.Compiled.IsDisposed, Is.False);
            for (var i = 0; i < 4; i++)
            {
                this.particles.RemoveFromHierarchy();
                Assert.That(this.particles.LiveCount, Is.Zero);
                this.parent.Add(this.particles);
                Assert.That(this.particles.IsPlaying, Is.False);
            }

            Assert.That(owner.Compiled.IsDisposed, Is.True);
            this.particles.Play();
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
        }

        [Test]
        public void AssetReplacementReleasesOldOwnerAndRestartsAnActiveRequest()
        {
            using var owner = new UIParticleRuntime(this.effect);
            var replacement = ScriptableObject.CreateInstance<UIParticleEffect>();
            try
            {
                replacement.Emitters[0].Bursts.Clear();
                this.particles.Play();
                owner.Dispose();
                this.particles.Effect = replacement;
                Assert.That(owner.Compiled.IsDisposed, Is.True);
                Assert.That(this.particles.IsPlaying, Is.True);
                Assert.That(this.particles.LiveCount, Is.Zero);
                this.particles.Effect = null;
                Assert.That(this.particles.LiveCount, Is.Zero);
                this.particles.Clear();
                this.particles.Effect = this.effect;
                Assert.That(this.particles.IsPlaying, Is.False);
            }
            finally
            {
                this.particles.Effect = null;
                Object.DestroyImmediate(replacement);
            }
        }

        [Test]
        public void ZeroSpeedDoesNotConsumeTheInitialBurstAndNonzeroSpeedResumes()
        {
            this.particles.PlaybackSpeed = 0;
            this.particles.Play();
            this.particles.Advance(20);
            Assert.That(this.particles.LiveCount, Is.Zero);
            this.particles.PlaybackSpeed = 1;
            this.particles.Advance(0);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            this.particles.PlaybackSpeed = 0;
            this.particles.Advance(20);
            this.particles.PlaybackSpeed = 1;
            this.particles.Advance(0.01);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidPlaybackSpeedIsRejected(float speed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.particles.PlaybackSpeed = speed);
        }

        [UnityTest]
        public IEnumerator HiddenAncestorAndExplicitPauseRemainIndependent()
        {
            this.particles.Play();
            this.parent.style.display = DisplayStyle.None;
            yield return null;
            this.particles.Advance(2);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            Assert.That(this.particles.IsPaused, Is.True);
            this.particles.Pause();
            this.parent.style.display = DisplayStyle.Flex;
            yield return null;
            this.particles.Advance(2);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            this.particles.Resume();
            this.particles.Advance(2);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16), "Showing drops hidden elapsed time");
            this.particles.Advance(0.05);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
        }

        [UnityTest]
        public IEnumerator ContinueAgesHiddenParticlesAndStopAndClearDoesNotRestartOnShow()
        {
            this.particles.HiddenBehaviour = UIParticleHiddenBehaviour.Continue;
            this.particles.Play();
            this.parent.style.visibility = Visibility.Hidden;
            yield return null;
            for (var i = 0; i < 20; i++)
            {
                this.particles.Advance(0.06);
            }

            Assert.That(this.particles.LiveCount, Is.Zero);
            this.particles.HiddenBehaviour = UIParticleHiddenBehaviour.StopAndClear;
            this.particles.Play();
            Assert.That(this.particles.IsPlaying, Is.False);
            this.parent.style.visibility = Visibility.Visible;
            yield return null;
            this.particles.Advance(0.1);
            Assert.That(this.particles.IsPlaying, Is.False);
        }

        [UnityTest]
        public IEnumerator ClearCancelsPendingLayoutStart()
        {
            var pending = new AnchorParticles
            {
                Effect = this.effect,
            };
            this.parent.Add(pending);
            pending.Play();
            Assert.That(pending.LiveCount, Is.Zero);
            pending.Clear();
            pending.style.width = 100;
            pending.style.height = 100;
            yield return null;
            pending.Advance(0);
            Assert.That(pending.IsPlaying, Is.False);
            Assert.That(pending.LiveCount, Is.Zero);
        }

        [Test]
        public void EffectsDisabledCancelsAndDoesNotReplayOnEnable()
        {
            var completed = 0;
            this.particles.Completed += () => completed++;
            this.particles.Play();
            this.particles.EffectsEnabled = false;
            this.particles.Play();
            this.particles.EffectsEnabled = true;
            this.particles.Advance(1);
            Assert.That(this.particles.IsPlaying, Is.False);
            Assert.That(this.particles.LiveCount, Is.Zero);
            Assert.That(completed, Is.Zero);
        }

        [Test]
        public void NaturalCompletionCanRestartAndIsSentOncePerRun()
        {
            var completed = 0;
            this.particles.Completed += () =>
            {
                completed++;
                if (completed == 1)
                {
                    this.particles.Play();
                }
            };
            this.particles.Play();
            for (var i = 0; i < 50; i++)
            {
                this.particles.Advance(0.06);
            }

            Assert.That(completed, Is.EqualTo(2));
            Assert.That(this.particles.IsPlaying, Is.False);
            Assert.That(this.particles.LiveCount, Is.Zero);
        }

        [Test]
        public void ClearDetachAndReplacementCancelQueuedCompletion()
        {
            var completed = 0;
            this.particles.Completed += () => completed++;
            for (var operation = 0; operation < 3; operation++)
            {
                this.particles.Play();
                for (var i = 0; i < 20; i++)
                {
                    this.particles.Tick(0.06);
                }

                if (operation == 0)
                {
                    this.particles.Clear();
                }
                else if (operation == 1)
                {
                    this.particles.RemoveFromHierarchy();
                    this.parent.Add(this.particles);
                }
                else
                {
                    this.particles.Effect = null;
                }

                this.particles.DispatchCompletion();
            }

            Assert.That(completed, Is.Zero);
        }

        [Test]
        public void SourcePointRejectsDetachedAndOtherPanelSources()
        {
            Assert.Throws<ArgumentException>(() => this.particles.SetSourcePoint(new VisualElement(), Vector2.zero));
            var other = ScriptableObject.CreateInstance<EditorWindow>();
            try
            {
                other.Show();
                Assert.Throws<ArgumentException>(() => this.particles.SetSourcePoint(other.rootVisualElement, Vector2.zero));
                Assert.DoesNotThrow(() => this.particles.SetSourcePoint(this.parent, Vector2.one));
            }
            finally
            {
                other.Close();
            }
        }

        [UnityTest]
        public IEnumerator SingularTransformDefersInitialBirthUntilUsable()
        {
            this.parent.style.scale = new Scale(new Vector3(0, 1, 1));
            yield return null;
            this.particles.Play();
            Assert.That(this.particles.LiveCount, Is.Zero);
            this.parent.style.scale = new Scale(Vector3.one);
            yield return null;
            this.particles.Advance(0);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
        }

        [Test]
        public void ExplicitPauseAndClearCannotBeResumedIntoAnotherRun()
        {
            this.particles.Play();
            this.particles.Pause();
            this.particles.Advance(1);
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            this.particles.Clear();
            this.particles.Clear();
            this.particles.Resume();
            Assert.That(this.particles.IsPlaying, Is.False);
            Assert.That(this.particles.LiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator VisualGenerationReleaseAndAppDisposeReleaseAttachedParticleOwners()
        {
            using var app = new TestAnchorAppScope();
            for (var generation = 0; generation < 2; generation++)
            {
                app.App.SetPanel(new ParticlePanel());
                var root = app.App.RootVisualElement;
                this.window.rootVisualElement.Add(root);
                root.Add(this.particles);
                yield return null;
                using var owner = new UIParticleRuntime(this.effect);
                this.particles.Play();
                owner.Dispose();
                Assert.That(owner.Compiled.IsDisposed, Is.False);
                if (generation == 0)
                {
                    app.App.ReleaseVisualGeneration();
                }
                else
                {
                    app.App.Dispose();
                    app.App.Dispose();
                }

                Assert.That(owner.Compiled.IsDisposed, Is.True);
                Assert.That(this.particles.LiveCount, Is.Zero);
                Assert.That(this.particles.panel, Is.Not.Null, "App teardown owns resources even before the host removes its tree");
                root.RemoveFromHierarchy();
            }
        }

        [UnityTest]
        public IEnumerator PausedPanelTransformRepaintsWithoutAdvancingParticles()
        {
            var repaints = 0;
            this.particles.generateVisualContent += _ => repaints++;
            this.particles.SimulationSpace = UIParticleSpace.Panel;
            this.particles.Play();
            this.particles.Pause();
            yield return null;
            yield return null;
            var before = repaints;
            this.particles.Advance(1);
            yield return null;
            yield return null;
            Assert.That(repaints, Is.EqualTo(before));
            this.parent.style.translate = new Translate(25, 10);
            this.particles.Advance(1);
            yield return null;
            yield return null;
            Assert.That(repaints, Is.GreaterThan(before));
            Assert.That(this.particles.LiveCount, Is.EqualTo(16));
            Assert.That(this.particles.IsPaused, Is.True);
        }

        [UnityTest]
        public IEnumerator PausedTintAndFinalDeathInvalidateButUnchangedEmptyStateDoesNot()
        {
            var repaints = 0;
            this.particles.generateVisualContent += _ => repaints++;
            this.particles.Play();
            this.particles.Pause();
            yield return null;
            yield return null;
            var before = repaints;
            this.particles.Tint = Color.red;
            yield return null;
            yield return null;
            Assert.That(repaints, Is.GreaterThan(before));
            before = repaints;
            this.particles.Resume();
            this.particles.Advance(0.0625);
            this.particles.Advance(0.0625);
            Assert.That(this.particles.LiveCount, Is.Zero);
            yield return null;
            yield return null;
            Assert.That(repaints, Is.GreaterThan(before));
            before = repaints;
            this.particles.Advance(1);
            this.particles.Clear();
            this.particles.Clear();
            yield return null;
            yield return null;
            Assert.That(repaints, Is.EqualTo(before));
        }

        private sealed class ParticlePanel : IAnchorPanel
        {
            public VisualElement RootVisualElement { get; } = new();
            public string Theme { get; set; }
            public string Scale { get; set; }
        }
    }
}
