namespace BovineLabs.Anchor.Particles
{
    internal struct UIParticleEmitterState
    {
        public double Time;
        public int Count;
        public bool Started;
        public uint Ordinal;
        public ulong Attempted;
        public ulong Emitted;
        public ulong Dropped;
    }
}
