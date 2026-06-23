// <copyright file="AudioService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Core;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal sealed class AudioService : IAudioService, IDisposable
    {
        private readonly Dictionary<string, AnchorAudioProfile> profiles;
        private readonly HashSet<string> missingProfileWarnings = new(StringComparer.Ordinal);

        private GameObject host;
        private AudioSource source;

        public AudioService()
            : this(AnchorSettings.I.Audio)
        {
        }

        internal AudioService(AnchorAudioSettings settings)
        {
            this.profiles = (settings ?? new AnchorAudioSettings()).CreateProfileDictionary();
        }

        internal AudioSource Source => this.source;

        /// <inheritdoc />
        public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
        {
            switch (cueOverride.Mode)
            {
                case AnchorAudioOverrideMode.Disabled:
                    return;
                case AnchorAudioOverrideMode.Custom:
                    this.PlayOneShot(cueOverride.Clip);
                    return;
                default:
                    this.PlayOneShot(this.ResolveClip(profileKey, cue));
                    return;
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            var audioSource = this.EnsureSource();
            audioSource.clip = clip;
            audioSource.PlayOneShot(clip);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.host != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(this.host);
                }
                else
                {
                    Object.DestroyImmediate(this.host);
                }
            }

            this.host = null;
            this.source = null;
        }

        private AudioClip ResolveClip(string profileKey, AnchorAudioCue cue)
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

        private AudioSource EnsureSource()
        {
            if (this.source != null)
            {
                return this.source;
            }

            this.host = new GameObject("Anchor UI Audio");
#if UNITY_EDITOR
            this.host.hideFlags = HideFlags.HideAndDontSave;

            if (Application.isPlaying)
#endif
            {
                Object.DontDestroyOnLoad(this.host);
            }

            this.source = this.host.AddComponent<AudioSource>();
            this.source.playOnAwake = false;
            this.source.spatialBlend = 0f;
            this.source.loop = false;
            return this.source;
        }
    }
}
