// <copyright file="AnchorAudioSettingsTestUtility.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BovineLabs.Anchor.Audio;
    using NUnit.Framework;

    internal static class AnchorAudioSettingsTestUtility
    {
        private static readonly FieldInfo ProfilesField = typeof(AnchorAudioSettings).GetField(
            "profiles",
            BindingFlags.Instance | BindingFlags.NonPublic);

        internal static AnchorAudioSettings CreateSettings(params AnchorAudioProfile[] profiles)
        {
            var settings = new AnchorAudioSettings();
            SetProfiles(settings, profiles);
            return settings;
        }

        internal static AnchorAudioSettings CreateSettings(params KeyValuePair<string, AnchorAudioProfile>[] profiles)
        {
            var settings = new AnchorAudioSettings();
            SetProfiles(settings, profiles);
            return settings;
        }

        private static void SetProfiles(AnchorAudioSettings settings, IEnumerable<AnchorAudioProfile> profiles)
        {
            Assert.IsNotNull(ProfilesField);
            if (ProfilesField.FieldType == typeof(Dictionary<string, AnchorAudioProfile>))
            {
                var dictionary = new Dictionary<string, AnchorAudioProfile>(StringComparer.Ordinal);
                foreach (var profile in profiles)
                {
                    if (profile == null || string.IsNullOrWhiteSpace(profile.Key))
                    {
                        continue;
                    }

                    dictionary.Add(profile.Key, profile);
                }

                ProfilesField.SetValue(settings, dictionary);
                return;
            }

            ProfilesField.SetValue(settings, new List<AnchorAudioProfile>(profiles));
        }

        private static void SetProfiles(AnchorAudioSettings settings, IEnumerable<KeyValuePair<string, AnchorAudioProfile>> profiles)
        {
            Assert.IsNotNull(ProfilesField);
            if (ProfilesField.FieldType == typeof(Dictionary<string, AnchorAudioProfile>))
            {
                var dictionary = new Dictionary<string, AnchorAudioProfile>(StringComparer.Ordinal);
                foreach (var profile in profiles)
                {
                    if (string.IsNullOrWhiteSpace(profile.Key))
                    {
                        continue;
                    }

                    dictionary.Add(profile.Key, profile.Value);
                }

                ProfilesField.SetValue(settings, dictionary);
                return;
            }

            var list = new List<AnchorAudioProfile>();
            foreach (var profile in profiles)
            {
                if (profile.Value == null)
                {
                    continue;
                }

                profile.Value.Key = profile.Key;
                list.Add(profile.Value);
            }

            ProfilesField.SetValue(settings, list);
        }
    }
}
