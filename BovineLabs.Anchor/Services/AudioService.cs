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
        private readonly Dictionary<string, AnchorAudioProfile> _profiles;
        private readonly HashSet<string> _missingProfileWarnings = new(StringComparer.Ordinal);

        private GameObject _host;
        private AudioSource _source;

        public AudioService()
            : this(AnchorSettings.I.Audio)
        {
        }

        internal AudioService(AnchorAudioSettings settings)
        {
            _profiles = (settings ?? new AnchorAudioSettings()).CreateProfileDictionary();
        }

        internal AudioSource Source => _source;

        public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
        {
            switch (cueOverride.Mode)
            {
                case AnchorAudioOverrideMode.Disabled:
                    return;
                case AnchorAudioOverrideMode.Custom:
                    PlayOneShot(cueOverride.Clip);
                    return;
                default:
                    PlayOneShot(ResolveClip(profileKey, cue));
                    return;
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            var audioSource = EnsureSource();
            audioSource.clip = clip;
            audioSource.PlayOneShot(clip);
        }

        public void Dispose()
        {
            if (_host != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(_host);
                }
                else
                {
                    Object.DestroyImmediate(_host);
                }
            }

            _host = null;
            _source = null;
        }

        private AudioClip ResolveClip(string profileKey, AnchorAudioCue cue)
        {
            if (string.IsNullOrWhiteSpace(profileKey))
            {
                return null;
            }

            if (_profiles.TryGetValue(profileKey, out var profile))
            {
                return profile.GetClip(cue);
            }

            if (_missingProfileWarnings.Add(profileKey))
            {
                BLGlobalLogger.LogWarningString($"Anchor audio profile '{profileKey}' was not found. No audio will play for Anchor audio cues.");
            }

            return null;
        }

        private AudioSource EnsureSource()
        {
            if (_source != null)
            {
                return _source;
            }

            _host = new GameObject("Anchor UI Audio", typeof(AudioSource));
#if UNITY_EDITOR
            _host.hideFlags = HideFlags.HideAndDontSave;

            if (Application.isPlaying)
#endif
            {
                Object.DontDestroyOnLoad(_host);
            }

            _source = _host.GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.loop = false;
            return _source;
        }
    }
}
