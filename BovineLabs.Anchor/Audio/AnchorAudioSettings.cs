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

        [SerializeField]
        private Dictionary<string, AnchorAudioProfile> profiles = new()
        {
            { DefaultProfileKey, new AnchorAudioProfile { Key = DefaultProfileKey } },
        };

        internal Dictionary<string, AnchorAudioProfile> CreateProfileDictionary()
        {
            var profileDictionary = new Dictionary<string, AnchorAudioProfile>(StringComparer.Ordinal);

            foreach (var profile in this.profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.Key))
                {
                    continue;
                }

                profileDictionary.Add(profile.Key, profile.Value);
            }

            return profileDictionary;
        }
    }
}
