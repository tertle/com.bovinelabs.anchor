namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AnchorParticlesTests
    {
        private EditorWindow window;
        private UIParticleEffect effect;

        [SetUp]
        public void SetUp()
        {
            this.window = ScriptableObject.CreateInstance<EditorWindow>();
            this.window.Show();
            this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();
        }

        [TearDown]
        public void TearDown()
        {
            this.window.Close();
            Object.DestroyImmediate(this.effect);
        }

        [Test]
        public void DetachReleasesLastNativeOwnerAndReattachDoesNotRestart()
        {
            using var owner = new UIParticleRuntime(this.effect);
            var compiled = owner.Compiled;
            var particles = new AnchorParticles
            {
                Effect = this.effect,
            };
            particles.Play();
            Assert.That(particles.IsPlaying, Is.False);
            this.window.rootVisualElement.Add(particles);
            Assert.That(particles.LiveCount, Is.EqualTo(16));
            owner.Dispose();
            Assert.That(compiled.IsDisposed, Is.False);
            particles.RemoveFromHierarchy();
            Assert.That(compiled.IsDisposed, Is.True);
            Assert.That(particles.LiveCount, Is.Zero);
            this.window.rootVisualElement.Add(particles);
            Assert.That(particles.IsPlaying, Is.False);
            particles.Play();
            Assert.That(particles.LiveCount, Is.EqualTo(16));
        }

        [Test]
        public void AssetReplacementClearsPlaybackAndReleasesItsRevision()
        {
            using var owner = new UIParticleRuntime(this.effect);
            var particles = new AnchorParticles
            {
                Effect = this.effect,
            };
            this.window.rootVisualElement.Add(particles);
            particles.Play();
            owner.Dispose();
            particles.Effect = null;
            Assert.That(owner.Compiled.IsDisposed, Is.True);
            Assert.That(particles.LiveCount, Is.Zero);
            particles.Play();
            Assert.That(particles.IsPlaying, Is.False);
        }

        [Test]
        public void RepaintDoesNotAdvancePausedParticlesAndClearCannotBeResumed()
        {
            var particles = new AnchorParticles
            {
                Effect = this.effect,
            };
            this.window.rootVisualElement.Add(particles);
            particles.Play();
            particles.Pause();
            for (var i = 0; i < 60; i++)
            {
                particles.MarkDirtyRepaint();
                particles.Advance(1d / 60);
            }

            Assert.That(particles.LiveCount, Is.EqualTo(16));
            particles.Clear();
            particles.Resume();
            Assert.That(particles.IsPlaying, Is.False);
            Assert.That(particles.LiveCount, Is.Zero);
        }
    }
}
