namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;

    internal struct UIParticleEmitter
    {
        public int Offset;
        public int Capacity;
        public int BurstOffset;
        public int BurstCount;
        public int StepOffset;
        public int StepCount;
        public double Duration;
        public double Delay;
        public double Rate;
        public bool Looping;
        public UIParticleShape Shape;
        public float2 Position;
        public float2 Dimensions;
        public float Direction;
        public float Spread;
        public float2 Lifetime;
        public float2 Speed;
        public float2 Size;
        public float2 Rotation;
        public float2 AngularVelocity;
        public float4 ColorMin;
        public float4 ColorMax;
        public float2 Acceleration;
        public float Drag;
        public float4 Uv;
    }
}
