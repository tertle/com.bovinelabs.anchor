namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using System.Collections;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

    public class UIParticleAdmissionTests
    {
        private EditorWindow window;
        private UIParticleEffect effect;
        private AnchorParticles first;
        private AnchorParticles second;
        private UIParticleCoordinator coordinator;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            this.window = ScriptableObject.CreateInstance<EditorWindow>();
            this.window.Show();
            this.effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            this.first = new AnchorParticles
            {
                Effect = this.effect,
                PlayOnAttach = false,
            };
            this.second = new AnchorParticles
            {
                Effect = this.effect,
                PlayOnAttach = false,
            };
            this.first.style.width = this.second.style.width = 200;
            this.first.style.height = this.second.style.height = 200;
            this.window.rootVisualElement.Add(this.first);
            this.window.rootVisualElement.Add(this.second);
            this.coordinator = UIParticleCoordinator.Get(this.window.rootVisualElement.panel);
            this.coordinator.ParticleSlotBudget = 128;
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
        public void BudgetCountsCapacityAndPauseRetainsItUntilClear()
        {
            Assert.That(this.first.Play(), Is.EqualTo(ParticlePlayResult.Started));
            this.first.Pause();
            Assert.That(this.coordinator.ReservedSlots, Is.EqualTo(128));
            Assert.That(this.coordinator.LiveCount, Is.EqualTo(16));
            Assert.That(this.second.Play(), Is.EqualTo(ParticlePlayResult.BudgetExceeded));
            Assert.That(this.second.IsPlaying, Is.False);
            Assert.That(this.coordinator.RejectedPlays, Is.EqualTo(1));
            this.first.Clear();
            Assert.That(this.coordinator.ReservedSlots, Is.Zero);
            Assert.That(this.second.Play(), Is.EqualTo(ParticlePlayResult.Started));
        }

        [Test]
        public void FailedReplacementPreservesAssetParticlesSeedAndPause()
        {
            var replacement = ScriptableObject.CreateInstance<UIParticleEffect>();
            try
            {
                this.first.Play(7);
                this.first.Pause();
                Assert.That(this.first.Play(replacement, 9), Is.EqualTo(ParticlePlayResult.BudgetExceeded));
                Assert.That(this.first.Effect, Is.SameAs(this.effect));
                Assert.That(this.first.Seed, Is.EqualTo(7));
                Assert.That(this.first.IsPaused, Is.True);
                Assert.That(this.first.LiveCount, Is.EqualTo(16));
                this.first.Effect = replacement;
                Assert.That(this.first.Effect, Is.SameAs(this.effect));
                Assert.That(this.first.LastPlayResult, Is.EqualTo(ParticlePlayResult.BudgetExceeded));
                this.coordinator.ParticleSlotBudget = 256;
                Assert.That(this.first.Play(replacement, 9), Is.EqualTo(ParticlePlayResult.Started));
                Assert.That(this.first.Effect, Is.SameAs(replacement));
                Assert.That(this.coordinator.ReservedSlots, Is.EqualTo(128));
            }
            finally
            {
                this.first.Clear();
                Object.DestroyImmediate(replacement);
            }
        }

        [Test]
        public void RepeatedPlayReusesReservationAndDetachReleasesIt()
        {
            this.first.Play();
            for (var i = 0; i < 4; i++)
            {
                Assert.That(this.first.Play(), Is.EqualTo(ParticlePlayResult.Started));
                Assert.That(this.coordinator.ReservedSlots, Is.EqualTo(128));
            }

            this.first.RemoveFromHierarchy();
            Assert.That(this.coordinator.ReservedSlots, Is.Zero);
            Assert.That(this.second.Play(), Is.EqualTo(ParticlePlayResult.Started));
            this.second.RemoveFromHierarchy();
            Assert.That(this.coordinator.ReservedSlots, Is.Zero);
            Assert.That(UIParticleCoordinator.Get(this.window.rootVisualElement.panel).ParticleSlotBudget, Is.EqualTo(128));
        }

        [Test]
        public void BudgetCannotShrinkBelowResidentStorage()
        {
            this.first.Play();
            Assert.Throws<ArgumentOutOfRangeException>(() => this.coordinator.ParticleSlotBudget = 127);
            Assert.That(this.coordinator.ParticleSlotBudget, Is.EqualTo(128));
            this.first.Clear();
            this.coordinator.ParticleSlotBudget = 0;
            Assert.That(this.first.Play(), Is.EqualTo(ParticlePlayResult.BudgetExceeded));
        }

        [Test]
        public void DisabledPlayDoesNotReserveOrReplay()
        {
            this.first.EffectsEnabled = false;
            Assert.That(this.first.Play(), Is.EqualTo(ParticlePlayResult.Suppressed));
            this.first.EffectsEnabled = true;
            this.first.Advance(0);
            Assert.That(this.first.IsPlaying, Is.False);
            Assert.That(this.coordinator.ReservedSlots, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DeferredAttachmentRejectsWithoutStaleReservation()
        {
            this.first.RemoveFromHierarchy();
            Assert.That(this.first.Play(), Is.EqualTo(ParticlePlayResult.Deferred));
            this.second.Play();
            this.window.rootVisualElement.Add(this.first);
            yield return null;
            this.first.Advance(0);
            Assert.That(this.first.LastPlayResult, Is.EqualTo(ParticlePlayResult.BudgetExceeded));
            Assert.That(this.first.IsPlaying, Is.False);
            Assert.That(this.coordinator.ReservedSlots, Is.EqualTo(128));
            this.second.Clear();
            this.first.Advance(0);
            Assert.That(this.first.LiveCount, Is.Zero, "Rejected requests are not replayed when capacity becomes available.");
        }
    }
}
