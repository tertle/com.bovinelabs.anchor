namespace BovineLabs.Anchor.Particles
{
    using Unity.Collections;

    internal struct UIParticleCompiledData
    {
        public NativeArray<UIParticleEmitter> Emitters;
        public NativeArray<UIParticleBurst> Bursts;
        public NativeArray<UIParticleSample> Samples;
        public NativeArray<UIParticleColorStep> Steps;
    }
}
