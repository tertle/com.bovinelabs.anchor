// <copyright file="AnchorAudioProfileResolverTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using AppUIButton = Unity.AppUI.UI.Button;
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
        public void ResolveClip_UnconfiguredAppUiPressable_ReturnsNull()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));

            Assert.IsNull(resolver.ResolveClip(new AppUIButton(), AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_UnconfiguredToolkitButton_ReturnsNull()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));

            Assert.IsNull(resolver.ResolveClip(new UIToolkitButton(), AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_AnchorButton_UsesDefaultProfile()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(hover, activate)));
            var button = new AnchorButton();

            Assert.AreSame(hover, resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_AnchorActionButton_UsesDefaultProfile()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(hover, activate)));
            var button = new AnchorActionButton();

            Assert.AreSame(hover, resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_EmptyProfileWithInheritedCues_ReturnsNull()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));
            var button = new AnchorButton { audioProfile = string.Empty };

            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_NamedProfile_ResolvesHoverAndActivate()
        {
            var hover = this.CreateClip("quiet-hover");
            var activate = this.CreateClip("quiet-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                DefaultProfile(),
                Profile("quiet", hover, activate)));
            var button = new AnchorButton { audioProfile = "quiet" };

            Assert.AreSame(hover, resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

#if UNITY_6000_6_OR_NEWER
        [Test]
        public void ResolveClip_DictionaryProfile_UsesDictionaryKey()
        {
            var activate = this.CreateClip("quiet-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                new[]
                {
                    new KeyValuePair<string, AnchorAudioProfile>("quiet", new AnchorAudioProfile { ActivateClip = activate }),
                }));
            var button = new AnchorButton { audioProfile = "quiet" };

            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }
#endif

        [Test]
        public void ResolveClip_MissingProfile_WarnsOnceAndReturnsNull()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));
            var button = new AnchorButton { audioProfile = "missing" };

            LogAssert.Expect(
                LogType.Warning,
                new Regex("Anchor audio profile 'missing' was not found\\. No audio will play for inherited Anchor audio cues\\."));

            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Activate));
            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Hover));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ResolveClip_CustomOverrideWithoutProfile_ReturnsCustomClip()
        {
            var customActivate = this.CreateClip("custom-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile()));
            var button = new AppUIButton();

            try
            {
                AnchorAudio.SetOptions(
                    button,
                    new AnchorAudioOptions
                    {
                        Profile = string.Empty,
                        Activate = AnchorAudioCueOverride.Custom(customActivate),
                    });

                Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Hover));
                Assert.AreSame(customActivate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
            }
            finally
            {
                AnchorAudio.ClearOptions(button);
            }
        }

        [Test]
        public void ResolveClip_DisabledOverrideSuppressesCueWhileOtherCueInherits()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(hover, activate)));
            var button = new AnchorButton
            {
                hoverAudioMode = AnchorAudioOverrideMode.Disabled,
            };

            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_AttachedDefaultOptions_OptRawAppUiControlIntoDefaultProfile()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));
            var button = new AppUIButton();

            try
            {
                AnchorAudio.SetOptions(button, new AnchorAudioOptions());

                Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));
            }
            finally
            {
                AnchorAudio.ClearOptions(button);
            }
        }

        [Test]
        public void ResolveClip_ClearingAttachedOptions_ReturnsRawControlToSilentBehavior()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));
            var button = new AppUIButton();

            AnchorAudio.SetOptions(button, new AnchorAudioOptions());
            Assert.AreSame(activate, resolver.ResolveClip(button, AnchorAudioCue.Activate));

            AnchorAudio.ClearOptions(button);
            Assert.IsNull(resolver.ResolveClip(button, AnchorAudioCue.Activate));
        }

#if !UNITY_6000_6_OR_NEWER
        [Test]
        public void BuildProfiles_DuplicateProfileKeys_ReportError()
        {
            LogAssert.Expect(LogType.Error, new Regex("Anchor audio profile 'default' is already registered\\."));

            _ = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(), DefaultProfile()));

            LogAssert.NoUnexpectedReceived();
        }
#endif

        private static AnchorAudioSettings CreateSettings(params AnchorAudioProfile[] profiles)
        {
            return AnchorAudioSettingsTestUtility.CreateSettings(profiles);
        }

        private static AnchorAudioSettings CreateSettings(params KeyValuePair<string, AnchorAudioProfile>[] profiles)
        {
            return AnchorAudioSettingsTestUtility.CreateSettings(profiles);
        }

        private static AnchorAudioProfile DefaultProfile(AudioClip hoverClip = null, AudioClip activateClip = null)
        {
            return Profile(AnchorAudio.DefaultProfileKey, hoverClip, activateClip);
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
    }
}
