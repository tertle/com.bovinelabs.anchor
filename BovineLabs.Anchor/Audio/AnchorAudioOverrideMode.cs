namespace BovineLabs.Anchor.Audio
{
    /// <summary>
    /// How a per-element cue value should resolve against inherited audio profiles.
    /// </summary>
    public enum AnchorAudioOverrideMode
    {
        /// <summary>Resolve from the selected named profile.</summary>
        Inherit,

        /// <summary>Suppress audio for this cue.</summary>
        Disabled,

        /// <summary>Use the explicitly assigned clip.</summary>
        Custom,
    }
}
