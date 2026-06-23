// <copyright file="AnchorAudioProfileResolverTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.Audio;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

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
        public void ResolveClip_DefaultProfile_ResolvesHoverAndActivate()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(hover, activate)));

            Assert.AreSame(hover, resolver.ResolveClip(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate));
        }

        [Test]
        public void ResolveClip_NamedProfile_ResolvesHoverAndActivate()
        {
            var hover = this.CreateClip("quiet-hover");
            var activate = this.CreateClip("quiet-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                DefaultProfile(),
                Profile("quiet", hover, activate)));

            Assert.AreSame(hover, resolver.ResolveClip("quiet", AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip("quiet", AnchorAudioCue.Activate));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ResolveClip_EmptyProfileKeys_ReturnNullWithoutWarning(string profileKey)
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));

            Assert.IsNull(resolver.ResolveClip(profileKey, AnchorAudioCue.Hover));
            Assert.IsNull(resolver.ResolveClip(profileKey, AnchorAudioCue.Activate));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ResolveClip_MissingProfile_WarnsOnceAndReturnsNull()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));

            LogAssert.Expect(
                LogType.Warning,
                new Regex("Anchor audio profile 'missing' was not found\\. No audio will play for Anchor audio cues\\."));

            Assert.IsNull(resolver.ResolveClip("missing", AnchorAudioCue.Activate));
            Assert.IsNull(resolver.ResolveClip("missing", AnchorAudioCue.Hover));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ResolveClip_NullCueClip_ReturnsNullWithoutWarning()
        {
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(activateClip: activate)));

            Assert.IsNull(resolver.ResolveClip(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Hover));
            Assert.AreSame(activate, resolver.ResolveClip(AnchorAudioSettings.DefaultProfileKey, AnchorAudioCue.Activate));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ResolveClip_ProfileMatching_IsOrdinalAndCaseSensitive()
        {
            var hover = this.CreateClip("quiet-hover");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(Profile("Quiet", hover)));

            LogAssert.Expect(
                LogType.Warning,
                new Regex("Anchor audio profile 'quiet' was not found\\. No audio will play for Anchor audio cues\\."));

            Assert.AreSame(hover, resolver.ResolveClip("Quiet", AnchorAudioCue.Hover));
            Assert.IsNull(resolver.ResolveClip("quiet", AnchorAudioCue.Hover));
            LogAssert.NoUnexpectedReceived();
        }

#if UNITY_6000_6_OR_NEWER
        [Test]
        public void ResolveClip_DictionaryProfile_UsesDictionaryKey()
        {
            var activate = this.CreateClip("quiet-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(
                new[]
                {
                    new KeyValuePair<string, AnchorAudioProfile>("quiet", new AnchorAudioProfile { Key = "serialized-key", ActivateClip = activate }),
                }));

            Assert.AreSame(activate, resolver.ResolveClip("quiet", AnchorAudioCue.Activate));
        }
#endif

        [Test]
        public void ResolveClip_UnknownCue_ReturnsNull()
        {
            var hover = this.CreateClip("default-hover");
            var activate = this.CreateClip("default-activate");
            var resolver = new AnchorAudioProfileResolver(CreateSettings(DefaultProfile(hover, activate)));

            Assert.IsNull(resolver.ResolveClip(AnchorAudioSettings.DefaultProfileKey, (AnchorAudioCue)int.MaxValue));
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
