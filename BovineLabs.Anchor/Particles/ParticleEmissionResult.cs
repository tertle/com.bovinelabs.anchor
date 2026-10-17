namespace BovineLabs.Anchor.Particles
{
    public struct ParticleEmissionResult
    {
        public ParticleEmissionStatus Status;
        public ulong Attempted;
        public int Emitted;
        public ulong Dropped;
    }
}
