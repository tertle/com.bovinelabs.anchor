// <copyright file="AnchorAudioProfileResolverTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;
    using AppUIButton = Unity.AppUI.UI.Button;
    using AppUIToggle = Unity.AppUI.UI.Toggle;
    using Object = UnityEngine.Object;
    using UIToolkitButton = UnityEngine.UIElements.Button;

    public class AnchorAudioProfileResolverTests
    {
        private readonly List<AudioClip> clips = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var clip in this.clips)
            {
                Object.DestroyImmediate(clip);
            }

            this.clips.Clear();
        }

        [Test]
        public void ResolveClip_AppUiPressable_UsesDefaultPressableProfile()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(CueProfile(hoverClip: hover, activateClip: activate)));
            var button = new AppUIButton();

            Assert.AreSame(hover, resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_NamedProfile_OverridesDefault()
        {
            var defaultActivate = this.CreateClip("default-activate");
            var quietActivate = this.CreateClip("quiet-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                CueProfile(activateClip: defaultActivate),
                Profile("quiet", activateClip: quietActivate)));
            var button = new AnchorButton { audioProfile = "quiet" };

            Assert.AreSame(quietActivate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_CueOverrides_ResolveIndependently()
        {
            var defaultHover = this.CreateClip("default-hover");
            var defaultActivate = this.CreateClip("default-activate");
            var customHover = this.CreateClip("custom-hover");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(CueProfile(hoverClip: defaultHover, activateClip: defaultActivate)));
            var button = new AnchorButton
            {
                hoverAudioMode = AnchorAudioOverrideMode.Custom,
                hoverAudioClip = customHover,
                activateAudioMode = AnchorAudioOverrideMode.Disabled,
            };

            Assert.AreSame(customHover, resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_MissingProfile_FallsBackToDefault()
        {
            var defaultActivate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(CueProfile(activateClip: defaultActivate)));
            var button = new AnchorButton { audioProfile = "missing" };

            Assert.AreSame(defaultActivate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_AttachedOptions_WorkOnRawAppUiControls()
        {
            var destructiveActivate = this.CreateClip("destructive-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                CueProfile(),
                Profile("destructive", activateClip: destructiveActivate)));
            var button = new AppUIButton();

            try
            {
                AnchorAudio.SetOptions(button, new AnchorAudioOptions { Profile = "destructive" });

                Assert.AreSame(destructiveActivate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
            }
            finally
            {
                AnchorAudio.ClearOptions(button);
            }
        }

        [Test]
        public void ResolveClip_TypeMapping_OverridesDefault()
        {
            var defaultActivate = this.CreateClip("default-activate");
            var toggleActivate = this.CreateClip("toggle-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                CueProfile(activateClip: defaultActivate),
                Profile("toggle", activateClip: toggleActivate)));
            resolver.RegisterTypeProfile(typeof(AppUIToggle), "toggle");

            Assert.AreSame(toggleActivate, resolver.ResolveClip(new AppUIToggle(), AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_UnsupportedPlainElement_StaysSilent()
        {
            var defaultActivate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(CueProfile(activateClip: defaultActivate)));

            Assert.IsNull(resolver.ResolveClip(new VisualElement(), AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_StandardToolkitButton_UsesDefaultPressableProfile()
        {
            var defaultActivate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(CueProfile(activateClip: defaultActivate)));

            Assert.AreSame(defaultActivate, resolver.ResolveClip(new UIToolkitButton(), AnchorAudioCue.Activate));
        }

        private static AnchorAudioSettings CreateSettings(AnchorAudioCueProfile defaultProfile, params AnchorAudioProfile[] profiles)
        {
            return new AnchorAudioSettings(defaultProfile, profiles ?? Array.Empty<AnchorAudioProfile>());
        }

        private static AnchorAudioCueProfile CueProfile(AudioClip hoverClip = null, AudioClip activateClip = null)
        {
            return new AnchorAudioCueProfile
            {
                HoverClip = hoverClip,
                ActivateClip = activateClip,
            };
        }

        private static AnchorAudioProfile Profile(string key = null, AudioClip hoverClip = null, AudioClip activateClip = null)
        {
            return new AnchorAudioProfile
            {
                Key = key ?? string.Empty,
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
    }
}
