// <copyright file="AnchorAudioProfile.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Named pair of hover and activation audio clips.
    /// </summary>
    [Serializable]
    public sealed class AnchorAudioProfile
    {
        [HideInInspector]
        [SerializeField]
        private string key = string.Empty;

        [SerializeField]
        private AudioClip hoverClip;

        [SerializeField]
        private AudioClip activateClip;

        /// <summary>Gets or sets the profile key.</summary>
        public string Key
        {
            get => this.key;
            set => this.key = value ?? string.Empty;
        }

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
            return cue switch
            {
                AnchorAudioCue.Hover => this.hoverClip,
                AnchorAudioCue.Activate => this.activateClip,
                _ => null,
            };
        }
    }
}
