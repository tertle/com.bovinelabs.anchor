namespace BovineLabs.Anchor.Services
{
    using BovineLabs.Anchor.Audio;

    public interface IAudioService
    {
        void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride);
    }
}
