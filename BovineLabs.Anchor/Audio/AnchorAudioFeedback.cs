// <copyright file="AnchorAudioFeedback.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;

    /// <summary>
    /// Plays resolved Anchor UI audio feedback.
    /// </summary>
    internal sealed class AnchorAudioFeedback
    {
        private readonly AnchorAudioProfileResolver resolver;
        private readonly IAnchorAudioService audioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorAudioFeedback"/> class.
        /// </summary>
        /// <param name="resolver">The profile resolver.</param>
        /// <param name="audioService">The playback service.</param>
        public AnchorAudioFeedback(AnchorAudioProfileResolver resolver, IAnchorAudioService audioService)
        {
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        /// <summary>
        /// Plays a resolved cue for a named profile.
        /// </summary>
        /// <param name="profileKey">The profile key to resolve.</param>
        /// <param name="cue">The cue to play.</param>
        public void Play(string profileKey, AnchorAudioCue cue)
        {
            var clip = this.resolver.ResolveClip(profileKey, cue);
            if (clip != null)
            {
                this.audioService.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Plays a cue from either a per-element override or a named profile.
        /// </summary>
        /// <param name="profileKey">The inherited profile key.</param>
        /// <param name="cue">The cue to play.</param>
        /// <param name="cueOverride">The per-element cue override.</param>
        public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
        {
            switch (cueOverride.Mode)
            {
                case AnchorAudioOverrideMode.Disabled:
                    return;
                case AnchorAudioOverrideMode.Custom:
                    if (cueOverride.Clip != null)
                    {
                        this.audioService.PlayOneShot(cueOverride.Clip);
                    }

                    return;
                default:
                    this.Play(profileKey, cue);
                    return;
            }
        }
    }
}
