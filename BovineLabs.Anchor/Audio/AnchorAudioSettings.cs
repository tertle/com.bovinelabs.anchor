// <copyright file="AnchorAudioSettings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Audio feedback configuration for Anchor UI.
    /// </summary>
    [Serializable]
    public sealed class AnchorAudioSettings
    {
        /// <summary>The standard Anchor UI audio profile key.</summary>
        public const string DefaultProfileKey = "default";

#if UNITY_6000_6_OR_NEWER
        [SerializeField]
        private Dictionary<string, AnchorAudioProfile> profiles = new()
        {
            { DefaultProfileKey, new AnchorAudioProfile { Key = DefaultProfileKey } },
        };
#else
        [SerializeField]
        private List<AnchorAudioProfile> profiles = new()
        {
            new() { Key = DefaultProfileKey },
        };
#endif

        internal Dictionary<string, AnchorAudioProfile> CreateProfileDictionary()
        {
            var profileDictionary = new Dictionary<string, AnchorAudioProfile>(StringComparer.Ordinal);

#if UNITY_6000_6_OR_NEWER
            foreach (var profile in this.profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.Key))
                {
                    continue;
                }

                profileDictionary.Add(profile.Key, profile.Value);
            }
#else
            foreach (var profile in this.profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.Key))
                {
                    continue;
                }

                if (!profileDictionary.TryAdd(profile.Key, profile))
                {
                    Core.BLGlobalLogger.LogError($"Anchor audio profile '{profile.Key}' is already registered.");
                }
            }
#endif

            return profileDictionary;
        }
    }
}
