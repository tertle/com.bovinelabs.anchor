// <copyright file="AudioServiceTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

    public class AudioServiceTests
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
        public void Play_DefaultProfile_ResolvesHoverAndActivate()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(hover, activate)));

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);
            Assert.AreSame(hover, service.Source.clip);

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);
            Assert.AreSame(activate, service.Source.clip);
        }

        [TestCase(null)]
        public void Play_EmptyProfileKeys_DoNotPlayOrWarn(string profileKey)
        {
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(activateClip: activate)));

            service.Play(profileKey, AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);
            service.Play(profileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);

            Assert.IsNull(service.Source);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Play_MissingProfile_WarnsOnceAndDoesNotPlay()
        {
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(activateClip: activate)));

            LogAssert.Expect(
                LogType.Warning,
                new Regex("Anchor audio profile 'missing' was not found\\. No audio will play for Anchor audio cues\\."));

            service.Play("missing", AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);
            service.Play("missing", AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);

            Assert.IsNull(service.Source);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Play_NullCueClip_DoesNotPlayOrWarn()
        {
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(activateClip: activate)));

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);
            Assert.IsNull(service.Source);

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);
            Assert.AreSame(activate, service.Source.clip);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Play_ProfileMatching_IsOrdinalAndCaseSensitive()
        {
            var hover = this.CreateClip("quiet-hover");
            using var service = new AudioService(CreateSettings(Profile("Quiet", hover)));

            LogAssert.Expect(
                LogType.Warning,
                new Regex("Anchor audio profile 'quiet' was not found\\. No audio will play for Anchor audio cues\\."));

            service.Play("Quiet", AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);
            Assert.AreSame(hover, service.Source.clip);

            service.Play("quiet", AnchorAudioCue.Hover, AnchorAudioCueOverride.Inherit);
            Assert.AreSame(hover, service.Source.clip);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Play_DictionaryProfile_UsesDictionaryKey()
        {
            var activate = this.CreateClip("quiet-activate");
            using var service = new AudioService(CreateSettings(
                new[]
                {
                    new KeyValuePair<string, AnchorAudioProfile>("quiet", new AnchorAudioProfile { Key = "serialized-key", ActivateClip = activate }),
                }));

            service.Play("quiet", AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);

            Assert.AreSame(activate, service.Source.clip);
        }

        [Test]
        public void Play_UnknownCue_DoesNotPlay()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(hover, activate)));

            service.Play(AnchorAudioSettings.DefaultProfileKey, (AnchorAudioCue)int.MaxValue, AnchorAudioCueOverride.Inherit);

            Assert.IsNull(service.Source);
        }

        [Test]
        public void Play_CreatesHiddenHost()
        {
            var activate = this.CreateClip("default-activate");
            using var service = new AudioService(CreateSettings(DefaultProfile(activateClip: activate)));

            service.Play(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate, AnchorAudioCueOverride.Inherit);

            Assert.IsNotNull(service.Source);
#if UNITY_EDITOR
            Assert.AreEqual(HideFlags.HideAndDontSave, service.Source.gameObject.hideFlags);
#endif
        }

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
    }
}
