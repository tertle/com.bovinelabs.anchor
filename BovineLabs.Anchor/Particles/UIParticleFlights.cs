namespace BovineLabs.Anchor.Particles
{
    using Unity.Collections;
    using Unity.Mathematics;

    // One control's optional native state and its once-per-update panel-space target sample.
    internal struct UIParticleFlights
    {
        public NativeArray<UIParticleFlightState> States;
        public float2 Target;
        public int Arrivals;
    }
}
