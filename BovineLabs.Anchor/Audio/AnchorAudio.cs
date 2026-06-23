// <copyright file="AnchorAudio.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using BovineLabs.Anchor;

    /// <summary>
    /// Public facade for playing Anchor UI audio feedback from controls.
    /// </summary>
    public static class AnchorAudio
    {
        /// <summary>
        /// Plays a cue from either a per-element override or a named audio profile.
        /// </summary>
        /// <param name="profileKey">The profile key to resolve when <paramref name="cueOverride"/> inherits.</param>
        /// <param name="cue">The cue to play.</param>
        /// <param name="cueOverride">The per-element cue override.</param>
        public static void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
        {
            if (cueOverride.Mode == AnchorAudioOverrideMode.Disabled)
            {
                return;
            }

            if (cueOverride.Mode == AnchorAudioOverrideMode.Inherit && string.IsNullOrWhiteSpace(profileKey))
            {
                return;
            }

            if (TryGetFeedback(out var feedback))
            {
                feedback.Play(profileKey, cue, cueOverride);
            }
        }

        private static bool TryGetFeedback(out AnchorAudioFeedback feedback)
        {
            var services = AnchorApp.Current?.Services;
            if (services == null)
            {
                feedback = null;
                return false;
            }

            try
            {
                feedback = services.GetService(typeof(AnchorAudioFeedback)) as AnchorAudioFeedback;
            }
            catch (ObjectDisposedException)
            {
                feedback = null;
                return false;
            }

            return feedback != null;
        }
    }
}
