namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct AnchorAudioCueOverride : IEquatable<AnchorAudioCueOverride>
    {
        [SerializeField]
        private AnchorAudioOverrideMode _mode;

        [SerializeField]
        private AudioClip _clip;

        public AnchorAudioCueOverride(AnchorAudioOverrideMode mode, AudioClip clip = null)
        {
            _mode = mode;
            _clip = clip;
        }

        public static AnchorAudioCueOverride Inherit => new(AnchorAudioOverrideMode.Inherit);

        public static AnchorAudioCueOverride Disabled => new(AnchorAudioOverrideMode.Disabled);

        public AnchorAudioOverrideMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public AudioClip Clip
        {
            get => _clip;
            set => _clip = value;
        }

        public static AnchorAudioCueOverride Custom(AudioClip clip)
        {
            return new AnchorAudioCueOverride(AnchorAudioOverrideMode.Custom, clip);
        }

        public bool Equals(AnchorAudioCueOverride other)
        {
            return _mode == other._mode && _clip == other._clip;
        }

        public override bool Equals(object obj)
        {
            return obj is AnchorAudioCueOverride other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)_mode * 397) ^ (_clip != null ? _clip.GetHashCode() : 0);
            }
        }
    }
}
