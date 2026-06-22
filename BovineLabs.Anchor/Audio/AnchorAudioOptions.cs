// <copyright file="AnchorAudioOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    /// <summary>
    /// Runtime audio metadata that can be attached to third-party UI Toolkit elements.
    /// </summary>
    public sealed class AnchorAudioOptions
    {
        /// <summary>Gets or sets the named audio profile key.</summary>
        public string Profile { get; set; } = AnchorAudio.DefaultProfileKey;

        /// <summary>Gets or sets the hover cue override.</summary>
        public AnchorAudioCueOverride Hover { get; set; } = AnchorAudioCueOverride.Inherit;

        /// <summary>Gets or sets the activation cue override.</summary>
        public AnchorAudioCueOverride Activate { get; set; } = AnchorAudioCueOverride.Inherit;
    }
}
