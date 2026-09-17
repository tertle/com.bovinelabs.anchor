namespace BovineLabs.Anchor.Audio
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;

    public static class AnchorAudio
    {
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
