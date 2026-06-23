// <copyright file="AnchorAudioProfileResolver.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core;
    using UnityEngine;

    internal sealed class AnchorAudioProfileResolver
    {
        private readonly Dictionary<string, AnchorAudioProfile> profiles;
        private readonly HashSet<string> missingProfileWarnings = new(StringComparer.Ordinal);

        public AnchorAudioProfileResolver()
            : this(AnchorSettings.I.Audio)
        {
        }

        internal AnchorAudioProfileResolver(AnchorAudioSettings settings)
        {
            this.profiles = (settings ?? new AnchorAudioSettings()).CreateProfileDictionary();
        }

        internal AudioClip ResolveClip(string profileKey, AnchorAudioCue cue)
        {
            if (string.IsNullOrWhiteSpace(profileKey))
            {
                return null;
            }

            if (this.profiles.TryGetValue(profileKey, out var profile))
            {
                return profile.GetClip(cue);
            }

            if (this.missingProfileWarnings.Add(profileKey))
            {
                BLGlobalLogger.LogWarningString($"Anchor audio profile '{profileKey}' was not found. No audio will play for Anchor audio cues.");
            }

            return null;
        }
    }
}
