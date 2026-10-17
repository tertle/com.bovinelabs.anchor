namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;

    internal struct UIParticleFlightState
    {
        public float ScatterDuration;
        public float FlightDuration;
        public float Bend;
        public bool Started;
        public float2 Start;
        public float2 Control1;
        public float2 Control2;
        public float2 Destination;
    }
}
