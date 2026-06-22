// <copyright file="AnchorAudioOverrideMode.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    /// <summary>
    /// How a per-element cue value should resolve against inherited audio profiles.
    /// </summary>
    public enum AnchorAudioOverrideMode
    {
        /// <summary>Resolve from the selected profile, type mapping, or default pressable profile.</summary>
        Inherit,

        /// <summary>Suppress audio for this cue.</summary>
        Disabled,

        /// <summary>Use the explicitly assigned clip.</summary>
        Custom,
    }
}
