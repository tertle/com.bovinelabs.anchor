namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct AnchorAudioCueOverride : IEquatable<AnchorAudioCueOverride>
    {
        [SerializeField]
        private AnchorAudioOverrideMode mode;

        [SerializeField]
        private AudioClip clip;

        public AnchorAudioCueOverride(AnchorAudioOverrideMode mode, AudioClip clip = null)
        {
            this.mode = mode;
            this.clip = clip;
        }

        public static AnchorAudioCueOverride Inherit => new(AnchorAudioOverrideMode.Inherit);

        public static AnchorAudioCueOverride Disabled => new(AnchorAudioOverrideMode.Disabled);

        public AnchorAudioOverrideMode Mode
        {
            get => this.mode;
            set => this.mode = value;
        }

        public AudioClip Clip
        {
            get => this.clip;
            set => this.clip = value;
        }

        public static AnchorAudioCueOverride Custom(AudioClip clip)
        {
            return new AnchorAudioCueOverride(AnchorAudioOverrideMode.Custom, clip);
        }

        public bool Equals(AnchorAudioCueOverride other)
        {
            return this.mode == other.mode && this.clip == other.clip;
        }

        public override bool Equals(object obj)
        {
            return obj is AnchorAudioCueOverride other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)this.mode * 397) ^ (this.clip != null ? this.clip.GetHashCode() : 0);
            }
        }
    }
}
