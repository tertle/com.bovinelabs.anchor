// <copyright file="IAudioService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using BovineLabs.Anchor.Audio;

    /// <summary>
    /// Plays Anchor UI audio feedback.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Plays a cue from either a per-element override or a named audio profile.
        /// </summary>
        /// <param name="profileKey">The inherited profile key.</param>
        /// <param name="cue">The cue to play.</param>
        /// <param name="cueOverride">The per-element cue override.</param>
        void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride);
    }
}
