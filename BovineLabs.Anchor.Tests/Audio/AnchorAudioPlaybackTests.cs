// <copyright file="AnchorAudioPlaybackTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using AppUIActionButton = Unity.AppUI.UI.ActionButton;
    using AppUIButton = Unity.AppUI.UI.Button;
    using Object = UnityEngine.Object;
    using UIToolkitButton = UnityEngine.UIElements.Button;

    public class AnchorAudioPlaybackTests
    {
        private static readonly MethodInfo SetCurrentAppMethod = typeof(AnchorApp).GetMethod(
            "SetCurrentApp",
            BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo PointerIdSetMethod = typeof(PointerEnterEvent)
            .GetProperty(nameof(PointerEnterEvent.pointerId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetSetMethod(true);

        private readonly List<AudioClip> clips = new();
        private readonly List<EditorWindow> windows = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var window in this.windows)
            {
                if (window)
                {
                    window.Close();
                }
            }

            this.windows.Clear();

            foreach (var clip in this.clips)
            {
                Object.DestroyImmediate(clip);
            }

            this.clips.Clear();

            if (AnchorApp.Current != null)
            {
                AnchorApp.Current.Dispose();
            }
        }

        [Test]
        public void AnchorAudio_Play_NoCurrentApp_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => AnchorAudio.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit));
        }

        [Test]
        public void AnchorAudio_Play_NoFeedbackRegistration_DoesNotThrow()
        {
            using var scope = new TestAnchorAppScope();

            Assert.DoesNotThrow(() => AnchorAudio.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit));
        }

        [Test]
        public void AnchorAudio_Play_UsesCurrentAppService()
        {
            var firstClip = this.CreateClip("first");
            var firstService = new FakeAudioService();

            using (this.CreateScope(firstService, DefaultProfile(activateClip: firstClip)))
            {
                AnchorAudio.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);
            }

            var secondClip = this.CreateClip("second");
            var secondService = new FakeAudioService();

            using (this.CreateScope(secondService, DefaultProfile(activateClip: secondClip)))
            {
                AnchorAudio.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);
            }

            CollectionAssert.AreEqual(new[] { firstClip }, firstService.PlayedClips);
            CollectionAssert.AreEqual(new[] { secondClip }, secondService.PlayedClips);
        }

        [Test]
        public void AudioService_Play_OverrideModes_AreRespected()
        {
            var activate = this.CreateClip("activate");
            var custom = this.CreateClip("custom");
            using var service = new AudioService(CreateSettings(DefaultProfile(activateClip: activate)));

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Custom(custom));
            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Disabled);
            Assert.AreSame(custom, service.Source.clip);

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);

            Assert.AreSame(activate, service.Source.clip);
        }

        [Test]
        public void AnchorButton_DefaultProfile_PlaysHoverAndActivationOutsideNavHost()
        {
            var hover = this.CreateClip("hover");
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(hover, activate));
            var button = new AnchorButton();
            this.AttachToPanel(button);

            SendPointerEnter(button, PointerId.mousePointerId);
            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { hover, activate }, service.PlayedClips);
        }

        [TestCase("")]
        public void AnchorButton_EmptyProfile_DisablesBuiltInAudio(string profileKey)
        {
            var hover = this.CreateClip("hover");
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(hover, activate));
            var button = new AnchorButton { audioProfile = profileKey };
            this.AttachToPanel(button);

            SendPointerEnter(button, PointerId.mousePointerId);
            button.clickable.InvokePressed(null);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void AnchorButton_Hover_UsesMouseAndTrackedPointersButIgnoresTouch()
        {
            var hover = this.CreateClip("hover");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(hoverClip: hover));
            var button = new AnchorButton();
            this.AttachToPanel(button);

            SendPointerEnter(button, PointerId.mousePointerId);
            SendPointerEnter(button, PointerId.trackedPointerIdBase);
            SendPointerEnter(button, PointerId.touchPointerIdBase);

            CollectionAssert.AreEqual(new[] { hover, hover }, service.PlayedClips);
        }

        [Test]
        public void AnchorButton_Hover_DisabledButtonDoesNotPlay()
        {
            var hover = this.CreateClip("hover");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(hoverClip: hover));
            var button = new AnchorButton();
            button.SetEnabled(false);
            this.AttachToPanel(button);

            SendPointerEnter(button, PointerId.mousePointerId);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void AnchorButton_Hover_InternalChildEnterDoesNotReplay()
        {
            var hover = this.CreateClip("hover");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(hoverClip: hover));
            var button = new AnchorButton();
            var child = new VisualElement();
            button.Add(child);
            this.AttachToPanel(button);

            SendPointerEnter(button, PointerId.mousePointerId);
            SendPointerEnter(child, PointerId.mousePointerId);

            CollectionAssert.AreEqual(new[] { hover }, service.PlayedClips);
        }

        [Test]
        public void AnchorButton_InvokePressed_PlaysActivationOnce()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var button = new AnchorButton();

            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void AnchorButton_SubmitKey_PlaysActivationOnce()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var button = new AnchorButton();
            this.AttachToPanel(button);

            SendSubmitKeyUp(button);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void AnchorButton_CommandWithEventInfo_ExecutesOnceAndAudioPlaysOnce()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();
            var command = new CountingCommand();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var button = new AnchorButton { commandWithEventInfo = command };

            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, command.ExecuteCount);
            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void AnchorActionButton_Activation_ComesFromActionTriggeredEvent()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var button = new AnchorActionButton();
            this.AttachToPanel(button);

            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void AnchorActionButton_NestedActionTriggeredEvent_DoesNotActivateAncestorAudio()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var parent = new AnchorActionButton();
            var child = new AppUIActionButton();
            parent.hierarchy.Add(child);
            this.AttachToPanel(parent);

            using var evt = ActionTriggeredEvent.GetPooled();
            evt.target = child;
            child.SendEvent(evt);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void AnchorActionButton_ReplacingClickable_StillPlaysActivationOnce()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var button = new AnchorActionButton
            {
                clickable = new Pressable(),
            };
            this.AttachToPanel(button);

            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void RawControls_RemainSilent()
        {
            var activate = this.CreateClip("activate");
            var service = new FakeAudioService();

            using var scope = this.CreateScope(service, DefaultProfile(activateClip: activate));
            var appUiButton = new AppUIButton();
            var appUiActionButton = new AppUIActionButton();
            var uiToolkitButton = new UIToolkitButton();

            appUiButton.clickable.InvokePressed(null);
            appUiActionButton.clickable.InvokePressed(null);
            using (var evt = ClickEvent.GetPooled())
            {
                evt.target = uiToolkitButton;
                uiToolkitButton.SendEvent(evt);
            }

            Assert.IsEmpty(service.PlayedClips);
        }

        private static void SendPointerEnter(VisualElement element, int pointerId)
        {
            Assert.IsNotNull(PointerIdSetMethod);

            using var evt = PointerEnterEvent.GetPooled();
            PointerIdSetMethod.Invoke(evt, new object[] { pointerId });
            evt.target = element;
            element.SendEvent(evt);
        }

        private static void SendSubmitKeyUp(VisualElement element)
        {
            using var evt = KeyUpEvent.GetPooled('\0', KeyCode.Return, EventModifiers.None);
            evt.target = element;
            element.SendEvent(evt);
        }

        private TestAnchorAppScope CreateScope(FakeAudioService service, params AnchorAudioProfile[] profiles)
        {
            return new TestAnchorAppScope(services =>
            {
                service.SetProfiles(profiles);
                services.AddSingletonInstance(typeof(IAudioService), service);
            });
        }

        private void AttachToPanel(VisualElement element)
        {
            var window = ScriptableObject.CreateInstance<TestWindow>();
            this.windows.Add(window);
            window.Show();
            window.rootVisualElement.Add(element);
        }

        private static AnchorAudioSettings CreateSettings(params AnchorAudioProfile[] profiles)
        {
            return AnchorAudioSettingsTestUtility.CreateSettings(profiles);
        }

        private static AnchorAudioProfile DefaultProfile(AudioClip hoverClip = null, AudioClip activateClip = null)
        {
            return Profile(AnchorAudioSettings.DefaultProfileKey, hoverClip, activateClip);
        }

        private static AnchorAudioProfile Profile(string key, AudioClip hoverClip = null, AudioClip activateClip = null)
        {
            return new AnchorAudioProfile
            {
                Key = key,
                HoverClip = hoverClip,
                ActivateClip = activateClip,
            };
        }

        private AudioClip CreateClip(string name)
        {
            var clip = AudioClip.Create(name, 8, 1, 8000, false);
            this.clips.Add(clip);
            return clip;
        }

        private sealed class FakeAudioService : IAudioService
        {
            private Dictionary<string, AnchorAudioProfile> profiles = new(StringComparer.Ordinal);

            public List<AudioClip> PlayedClips { get; } = new();

            public void SetProfiles(params AnchorAudioProfile[] profiles)
            {
                this.profiles = CreateSettings(profiles).CreateProfileDictionary();
            }

            public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
            {
                switch (cueOverride.Mode)
                {
                    case AnchorAudioOverrideMode.Disabled:
                        return;
                    case AnchorAudioOverrideMode.Custom:
                        this.PlayOneShot(cueOverride.Clip);
                        return;
                    default:
                        this.PlayOneShot(this.ResolveClip(profileKey, cue));
                        return;
                }
            }

            private void PlayOneShot(AudioClip clip)
            {
                if (clip != null)
                {
                    this.PlayedClips.Add(clip);
                }
            }

            private AudioClip ResolveClip(string profileKey, AnchorAudioCue cue)
            {
                return !string.IsNullOrWhiteSpace(profileKey) && this.profiles.TryGetValue(profileKey, out var profile) ? profile.GetClip(cue) : null;
            }
        }

        private sealed class CountingCommand : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add
                {
                }

                remove
                {
                }
            }

            public int ExecuteCount { get; private set; }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                this.ExecuteCount++;
            }
        }

        private sealed class TestWindow : EditorWindow
        {
        }
    }
}
