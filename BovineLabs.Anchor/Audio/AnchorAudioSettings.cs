// <copyright file="AnchorAudioSettings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Audio feedback configuration for Anchor UI.
    /// </summary>
    [Serializable]
    public sealed class AnchorAudioSettings
    {
        [SerializeField]
        private AnchorAudioCueProfile defaultPressable = new();

        [SerializeField]
        private List<AnchorAudioProfile> profiles = new();

        /// <summary>Initializes a new instance of the <see cref="AnchorAudioSettings"/> class.</summary>
        public AnchorAudioSettings()
        {
        }

        internal AnchorAudioSettings(AnchorAudioCueProfile defaultPressable, IEnumerable<AnchorAudioProfile> profiles)
        {
            this.defaultPressable = defaultPressable ?? new AnchorAudioCueProfile();
            this.profiles = profiles != null ? new List<AnchorAudioProfile>(profiles) : new List<AnchorAudioProfile>();
        }

        /// <summary>Gets the default profile for AppUI pressable elements.</summary>
        public AnchorAudioCueProfile DefaultPressable => this.defaultPressable;

        /// <summary>Gets named audio profiles.</summary>
        public IReadOnlyList<AnchorAudioProfile> Profiles => this.profiles;
    }
}
