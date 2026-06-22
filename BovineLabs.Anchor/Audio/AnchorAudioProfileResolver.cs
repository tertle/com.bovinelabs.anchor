// <copyright file="AnchorAudioProfileResolver.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UIToolkitButton = UnityEngine.UIElements.Button;

    internal sealed class AnchorAudioProfileResolver
    {
        private readonly AnchorAudioSettings settings;
        private readonly Dictionary<string, AnchorAudioProfile> profiles = new(StringComparer.Ordinal);
        private readonly Dictionary<Type, string> typeProfiles = new();
        private readonly HashSet<string> missingProfileWarnings = new(StringComparer.Ordinal);

        public AnchorAudioProfileResolver()
            : this(AnchorSettings.I.Audio)
        {
        }

        internal AnchorAudioProfileResolver(AnchorAudioSettings settings)
        {
            this.settings = settings ?? new AnchorAudioSettings();
            this.BuildProfiles();
        }

        internal void RegisterTypeProfile(Type elementType, string profileKey)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            this.typeProfiles[elementType] = profileKey ?? string.Empty;
        }

        internal AudioClip ResolveClip(VisualElement element, AnchorAudioCue cue)
        {
            if (element == null)
            {
                return null;
            }

            var hasMetadata = TryGetElementOptions(element, out var profileKey, out var hoverOverride, out var activateOverride);
            if (!hasMetadata && element is not IPressable && element is not UIToolkitButton)
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

            return this.ResolveInheritedClip(element, profileKey, cue);
        }

        private static bool TryGetElementOptions(
            VisualElement element,
            out string profileKey,
            out AnchorAudioCueOverride hoverOverride,
            out AnchorAudioCueOverride activateOverride)
        {
            if (AnchorAudio.TryGetOptions(element, out var attached))
            {
                profileKey = attached.Profile;
                hoverOverride = attached.Hover;
                activateOverride = attached.Activate;
                return true;
            }

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

        private void BuildProfiles()
        {
            foreach (var profile in this.settings.Profiles)
            {
                if (profile == null || string.IsNullOrWhiteSpace(profile.Key))
                {
                    continue;
                }

                if (!this.profiles.TryAdd(profile.Key, profile))
                {
                    BLGlobalLogger.LogError($"Anchor audio profile '{profile.Key}' is already registered.");
                }
            }
        }

        private AudioClip ResolveInheritedClip(VisualElement element, string explicitProfileKey, AnchorAudioCue cue)
        {
            if (this.TryResolveProfileKey(explicitProfileKey, out var explicitProfile))
            {
                return explicitProfile.GetClip(cue);
            }

            if (this.TryResolveMappedProfile(element.GetType(), out var mappedProfile))
            {
                return mappedProfile.GetClip(cue);
            }

            return this.settings.DefaultPressable?.GetClip(cue);
        }

        private bool TryResolveMappedProfile(Type elementType, out AnchorAudioProfile profile)
        {
            if (this.typeProfiles.TryGetValue(elementType, out var exactKey))
            {
                return this.TryResolveProfileKey(exactKey, out profile);
            }

            foreach (var mapping in this.typeProfiles)
            {
                if (!mapping.Key.IsAssignableFrom(elementType))
                {
                    continue;
                }

                return this.TryResolveProfileKey(mapping.Value, out profile);
            }

            profile = null;
            return false;
        }

        private bool TryResolveProfileKey(string profileKey, out AnchorAudioProfile profile)
        {
            profile = null;
            if (string.IsNullOrWhiteSpace(profileKey))
            {
                return false;
            }

            if (this.profiles.TryGetValue(profileKey, out profile))
            {
                return true;
            }

            if (this.missingProfileWarnings.Add(profileKey))
            {
                BLGlobalLogger.LogWarningString($"Anchor audio profile '{profileKey}' was not found. Falling back to inherited audio.");
            }

            return false;
        }
    }
}
