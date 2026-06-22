// <copyright file="AnchorAudioCueOverride.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Per-element override for one audio cue.
    /// </summary>
    [Serializable]
    public struct AnchorAudioCueOverride : IEquatable<AnchorAudioCueOverride>
    {
        [SerializeField]
        private AnchorAudioOverrideMode mode;

        [SerializeField]
        private AudioClip clip;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorAudioCueOverride"/> struct.
        /// </summary>
        /// <param name="mode">The override mode.</param>
        /// <param name="clip">The custom clip, used only when <paramref name="mode"/> is <see cref="AnchorAudioOverrideMode.Custom"/>.</param>
        public AnchorAudioCueOverride(AnchorAudioOverrideMode mode, AudioClip clip = null)
        {
            this.mode = mode;
            this.clip = clip;
        }

        /// <summary>Gets an inherited cue override.</summary>
        public static AnchorAudioCueOverride Inherit => new(AnchorAudioOverrideMode.Inherit);

        /// <summary>Gets a disabled cue override.</summary>
        public static AnchorAudioCueOverride Disabled => new(AnchorAudioOverrideMode.Disabled);

        /// <summary>Gets or sets the override mode.</summary>
        public AnchorAudioOverrideMode Mode
        {
            get => this.mode;
            set => this.mode = value;
        }

        /// <summary>Gets or sets the custom clip.</summary>
        public AudioClip Clip
        {
            get => this.clip;
            set => this.clip = value;
        }

        /// <summary>Creates a custom cue override.</summary>
        /// <param name="clip">The clip to play.</param>
        /// <returns>A custom cue override.</returns>
        public static AnchorAudioCueOverride Custom(AudioClip clip)
        {
            return new AnchorAudioCueOverride(AnchorAudioOverrideMode.Custom, clip);
        }

        /// <inheritdoc />
        public bool Equals(AnchorAudioCueOverride other)
        {
            return this.mode == other.mode && this.clip == other.clip;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AnchorAudioCueOverride other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.mode, this.clip);
        }
    }
}
