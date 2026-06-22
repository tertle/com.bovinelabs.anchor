// <copyright file="AnchorAudioProfileResolver.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core;
    using UnityEngine;
    using UnityEngine.UIElements;

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

        internal AudioClip ResolveClip(VisualElement element, AnchorAudioCue cue)
        {
            if (element == null)
            {
                return null;
            }

            var hasMetadata = TryGetElementOptions(element, out var profileKey, out var hoverOverride, out var activateOverride);
            if (!hasMetadata)
            {
                return null;
            }

            var cueOverride = cue == AnchorAudioCue.Hover ? hoverOverride : activateOverride;
            switch (cueOverride.Mode)
            {
                case AnchorAudioOverrideMode.Disabled:
                    return null;
                case AnchorAudioOverrideMode.Custom:
                    return cueOverride.Clip;
            }

            return this.ResolveNamedProfileClip(profileKey, cue);
        }

        private static bool TryGetElementOptions(
            VisualElement element,
            out string profileKey,
            out AnchorAudioCueOverride hoverOverride,
            out AnchorAudioCueOverride activateOverride)
        {
            if (element is IAnchorAudioElement audioElement)
            {
                profileKey = audioElement.AudioProfile;
                hoverOverride = audioElement.HoverAudio;
                activateOverride = audioElement.ActivateAudio;
                return true;
            }

            profileKey = string.Empty;
            hoverOverride = AnchorAudioCueOverride.Inherit;
            activateOverride = AnchorAudioCueOverride.Inherit;
            return false;
        }

        private AudioClip ResolveNamedProfileClip(string profileKey, AnchorAudioCue cue)
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
                BLGlobalLogger.LogWarningString($"Anchor audio profile '{profileKey}' was not found. No audio will play for inherited Anchor audio cues.");
            }

            return null;
        }
    }
}
