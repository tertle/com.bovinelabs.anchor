namespace BovineLabs.Anchor.Tests.Components
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Components;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    public class AudioFeedbackTests
    {
        private RecordingAudioService _audio;
        private AnchorServiceProvider _provider;
        private AnchorApp _app;

        [SetUp]
        public void SetUp()
        {
            _audio = new RecordingAudioService();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(IAudioService), _audio);
            _provider = services.BuildServiceProvider();
            _app = new AnchorApp();
            _app.Initialize(_provider);
        }

        [TearDown]
        public void TearDown()
        {
            _app.Dispose();
            _provider.Dispose();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Button_PointerOrKeyboardActivation_PlaysOneActivateCue(bool keyboard)
        {
            var button = new Button();
            button.GetOrAddComponent<AudioFeedback>();
            using EventBase evt = keyboard ? KeyUpEvent.GetPooled() : PointerUpEvent.GetPooled();

            button.clickable.InvokePressed(evt);

            Assert.AreEqual(1, _audio.Calls.Count);
            Assert.AreEqual(AnchorAudioSettings.DefaultProfileKey, _audio.Calls[0].Profile);
            Assert.AreEqual(AnchorAudioCue.Activate, _audio.Calls[0].Cue);
            Assert.AreEqual(AnchorAudioOverrideMode.Inherit, _audio.Calls[0].Override.Mode);
        }

        [Test]
        public void Button_Activation_UsesCurrentComponentValues()
        {
            var button = new Button();
            ref var feedback = ref button.GetOrAddComponent<AudioFeedback>();
            feedback.AudioProfile = "updated";
            feedback.ActivateAudioMode = AnchorAudioOverrideMode.Disabled;
            button.NotifyComponentChanged(new BindingId("${component:BovineLabs.Anchor.Components.AudioFeedback}.AudioProfile"));
            button.NotifyComponentChanged(new BindingId("${component:BovineLabs.Anchor.Components.AudioFeedback}.ActivateAudioMode"));

            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, _audio.Calls.Count);
            Assert.AreEqual("updated", _audio.Calls[0].Profile);
            Assert.AreEqual(AnchorAudioOverrideMode.Disabled, _audio.Calls[0].Override.Mode);
        }

        [Test]
        public void Button_RemoveThenReadd_StopsOldSubscriptionAndPlaysOnce()
        {
            var button = new Button();
            button.GetOrAddComponent<AudioFeedback>();
            button.RemoveComponent<AudioFeedback>();

            button.clickable.InvokePressed(null);
            Assert.IsEmpty(_audio.Calls);

            button.GetOrAddComponent<AudioFeedback>();
            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, _audio.Calls.Count);
        }

        [Test]
        public void Button_ClickRemovesAudioBeforeItsCallback_DoesNotPlayOrAccessRemovedData()
        {
            var button = new Button();
            button.clicked += () => button.RemoveComponent<AudioFeedback>();
            button.GetOrAddComponent<AudioFeedback>();

            button.clickable.InvokePressed(null);

            Assert.IsEmpty(_audio.Calls);
        }

        [Test]
        public void AnchorButton_GetOrAddAuthoredAudio_ReusesDefaultWithoutDuplicatingActivation()
        {
            var button = new AnchorButton();
            ref var feedback = ref button.GetOrAddComponent<AudioFeedback>();
            feedback.AudioProfile = "authored";
            button.NotifyComponentChanged(new BindingId("${component:BovineLabs.Anchor.Components.AudioFeedback}.AudioProfile"));

            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, _audio.Calls.Count);
            Assert.AreEqual("authored", _audio.Calls[0].Profile);
        }

        [Test]
        public void AnchorButton_Activation_PreservesCommandEventInfo()
        {
            EventBase received = null;
            var button = new AnchorButton
            {
                commandWithEventInfo = new RelayCommand<EventBase>(evt => received = evt),
            };
            using var evt = KeyUpEvent.GetPooled();

            button.clickable.InvokePressed(evt);

            Assert.AreSame(evt, received);
            Assert.AreEqual(1, _audio.Calls.Count);
        }

        private sealed class RecordingAudioService : IAudioService
        {
            public List<(string Profile, AnchorAudioCue Cue, AnchorAudioCueOverride Override)> Calls { get; } = new();

            public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
            {
                Calls.Add((profileKey, cue, cueOverride));
            }
        }
    }
}
