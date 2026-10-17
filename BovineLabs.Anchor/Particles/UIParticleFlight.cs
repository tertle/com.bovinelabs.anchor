namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;

    internal struct UIParticleFlight
    {
        public bool Enabled;
        public int Offset;
        public float2 ScatterDuration;
        public float2 FlightDuration;
        public float PathBend;
        public float ArrivalRadius;
        public UIParticleTargetMode TargetMode;
    }
}
