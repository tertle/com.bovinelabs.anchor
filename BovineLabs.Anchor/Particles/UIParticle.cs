namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;

    internal struct UIParticle
    {
        public float2 Position;
        public float2 Velocity;
        public float Age;
        public float Lifetime;
        public float Size;
        public float Rotation;
        public float AngularVelocity;
        public float4 Tint;
    }
}
