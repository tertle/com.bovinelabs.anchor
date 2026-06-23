// <copyright file="AnchorAudio.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;

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
            if (TryGetAudioService(out var audioService))
            {
                audioService.Play(profileKey, cue, cueOverride);
            }
        }

        private static bool TryGetAudioService(out IAudioService audioService)
        {
            var services = AnchorApp.Current?.Services;
            if (services == null)
            {
                audioService = null;
                return false;
            }

            audioService = services.GetService<IAudioService>();
            return audioService != null;
        }
    }
}
