namespace BovineLabs.Anchor.Particles
{
    using System;
    using Unity.Mathematics;
    using UnityEngine;

    internal static class UIParticleSpriteUtility
    {
        internal static UIParticleSprite Compile(Vector2[] vertices, Vector2[] uv, ushort[] triangles, float pixelsPerUnit, Vector2 rectSize, float2 flip)
        {
            if (vertices.Length != 4 || uv.Length != 4 || triangles.Length != 6)
            {
                throw new ArgumentException("Particle sprites require rectangular four-vertex geometry. Use a Full Rect sprite; tight meshes are unsupported.");
            }

            var min = (float2)vertices[0];
            var max = min;
            var uvMin = (float2)uv[0];
            var uvMax = uvMin;
            for (var i = 1; i < 4; i++)
            {
                min = math.min(min, vertices[i]);
                max = math.max(max, vertices[i]);
                uvMin = math.min(uvMin, uv[i]);
                uvMax = math.max(uvMax, uv[i]);
            }

            if (math.any(max <= min) || math.any(uvMax <= uvMin))
            {
                throw new ArgumentException("Particle sprite geometry and UV region must have positive width and height.");
            }

            var corners = new float2x4();
            var seenVertices = 0;
            var seenUv = 0;
            var scale = pixelsPerUnit / math.cmax((float2)rectSize);
            for (var i = 0; i < 4; i++)
            {
                var p = ((float2)vertices[i] - min) / (max - min);
                var t = ((float2)uv[i] - uvMin) / (uvMax - uvMin);
                if (math.any(math.abs(p - math.round(p)) > 0.0001f) || math.any(math.abs(t - math.round(t)) > 0.0001f))
                {
                    throw new ArgumentException("Particle sprites require rectangular geometry and rectangular atlas UVs.");
                }

                var vertexCorner = (p.x > 0.5f ? 1 : 0) + (p.y > 0.5f ? 2 : 0);
                var uvCorner = (t.x > 0.5f ? 1 : 0) + (t.y > 0.5f ? 2 : 0);
                seenVertices |= 1 << vertexCorner;
                seenUv |= 1 << uvCorner;
                corners[uvCorner] = (float2)vertices[i] * new float2(scale, -scale) * flip;
            }

            if (seenVertices != 15 || seenUv != 15 || math.any(math.abs(corners.c0 + corners.c3 - corners.c1 - corners.c2) > 0.0001f))
            {
                throw new ArgumentException("Particle sprite corners must map to a rectangle without crossed UV edges.");
            }

            // Match each actual UV corner to its geometry. Packed rotations/reflections become a quad basis,
            // so the existing rectangular-UV renderer and adjacent-texture batching remain unchanged.
            return new UIParticleSprite
            {
                Enabled = true,
                Uv = new float4(uvMin, uvMax),
                Center = (corners.c0 + corners.c3) * 0.5f,
                Basis = new float2x2(corners.c3 - corners.c2, corners.c0 - corners.c2),
            };
        }

        internal static void Apply(in UIParticleSprite sprite, ref ParticleQuad quad)
        {
            math.sincos(quad.Angle, out var sin, out var cos);
            var rotation = new float2x2(cos, -sin, sin, cos);
            var basis = math.mul(float2x2.identity + quad.BasisOffset, rotation);
            quad.Position += math.mul(basis, sprite.Center * quad.Size);
            quad.BasisOffset = math.mul(basis, sprite.Basis) - float2x2.identity;
            quad.Angle = 0;
        }
    }
}
