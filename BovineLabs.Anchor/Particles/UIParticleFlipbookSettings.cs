namespace BovineLabs.Anchor.Particles
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class UIParticleFlipbookSettings
    {
        public bool Enabled;
        public int Columns = 1;
        public int Rows = 1;
        public UIParticleFlipbookMode Mode;
        public bool RandomRow;
        public int Row;
        [Tooltip("Inclusive frame range, sampled once per particle. Single Row counts frames within that row.")]
        public Vector2Int StartFrame;
        public UIParticleFlipbookTiming Timing;
        public float Cycles = 1;
        public float FramesPerSecond = 12;
        public bool Loop = true;
        [Tooltip("Inset on each edge of a frame, in source texture pixels. Author gutters for filtered textures.")]
        public float Padding;
    }
}
