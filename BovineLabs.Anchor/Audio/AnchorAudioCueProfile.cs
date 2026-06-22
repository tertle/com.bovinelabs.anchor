// <copyright file="AnchorAudioCueProfile.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Pair of hover and activation audio clips.
    /// </summary>
    [Serializable]
    public sealed class AnchorAudioCueProfile
    {
        [SerializeField]
        private AudioClip hoverClip;

        [SerializeField]
        private AudioClip activateClip;

        /// <summary>Gets or sets the hover clip.</summary>
        public AudioClip HoverClip
        {
            get => this.hoverClip;
            set => this.hoverClip = value;
        }

        /// <summary>Gets or sets the activation clip.</summary>
        public AudioClip ActivateClip
        {
            get => this.activateClip;
            set => this.activateClip = value;
        }

        internal AudioClip GetClip(AnchorAudioCue cue)
        {
            return cue == AnchorAudioCue.Hover ? this.hoverClip : this.activateClip;
        }
    }
}
