namespace BovineLabs.Anchor.Particles
{
    using Unity.Mathematics;
    using UnityEngine;

    public struct ParticleQuad
    {
        public float2 Position;
        public float2 Size;
        public float Angle;
        public Color32 Tint;
        // Texture coordinates: bottom-left XY, top-right ZW. UI coordinates have Y pointing down.
        public float4 Uv;
    }
}
