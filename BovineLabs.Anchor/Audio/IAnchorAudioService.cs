// <copyright file="IAnchorAudioService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using UnityEngine;

    /// <summary>
    /// Plays Anchor UI audio feedback.
    /// </summary>
    public interface IAnchorAudioService
    {
        /// <summary>
        /// Plays a one-shot audio clip.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        void PlayOneShot(AudioClip clip);
    }
}
