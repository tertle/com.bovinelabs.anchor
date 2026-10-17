namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;

    internal struct UIParticleFlipbook
    {
        public bool Enabled;
        public int Offset;
        public int Columns;
        public int Rows;
        public int FrameCount;
        public UIParticleFlipbookMode Mode;
        public bool RandomRow;
        public int Row;
        public int2 StartFrame;
        public UIParticleFlipbookTiming Timing;
        public float Rate;
        public bool Loop;
        public float2 Padding;
    }
}
