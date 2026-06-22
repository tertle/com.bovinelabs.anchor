// <copyright file="IAnchorAudioElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    /// <summary>
    /// Element-provided audio metadata for the NavHost scoped audio router.
    /// </summary>
    public interface IAnchorAudioElement
    {
        /// <summary>Gets the named audio profile key.</summary>
        string AudioProfile { get; }

        /// <summary>Gets the hover cue override.</summary>
        AnchorAudioCueOverride HoverAudio { get; }

        /// <summary>Gets the activation cue override.</summary>
        AnchorAudioCueOverride ActivateAudio { get; }
    }
}
