namespace BovineLabs.Anchor.Tests.Particles
{
    using System;
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class UIParticleSpriteTests
    {
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ActualUvCornersPreservePackedRotationPivotAndReflectedBirthBasis(bool packedRotation, bool flipX)
        {
            var geometry = new[] { new Vector2(-0.25f, -0.75f), new Vector2(0.75f, -0.75f), new Vector2(-0.25f, -0.25f), new Vector2(0.75f, -0.25f) };
            var uv = packedRotation
                ? new[] { new Vector2(0.6f, 0.3f), new Vector2(0.6f, 0.8f), new Vector2(0.2f, 0.3f), new Vector2(0.2f, 0.8f) }
                : new[] { new Vector2(0.2f, 0.3f), new Vector2(0.6f, 0.3f), new Vector2(0.2f, 0.8f), new Vector2(0.6f, 0.8f) };
            var flip = new float2(flipX ? -1 : 1, 1);
            var sprite = UIParticleSpriteUtility.Compile(geometry, uv, new ushort[] { 0, 2, 1, 1, 2, 3 }, 100, new Vector2(100, 100), flip);
            var birthBasis = new float2x2(-2, 0.3f, 0, 1);
            var origin = new float2(10, 20);
            var quad = new ParticleQuad
            {
                Position = origin,
                Size = new float2(8),
                Angle = math.PI * 0.5f,
                Uv = sprite.Uv,
                BasisOffset = birthBasis - float2x2.identity,
            };
            UIParticleSpriteUtility.Apply(in sprite, ref quad);
            var quads = new NativeArray<ParticleQuad>(1, Allocator.Temp);
            var vertices = new NativeArray<Vertex>(4, Allocator.Temp);
            var indices = new NativeArray<ushort>(6, Allocator.Temp);
            quads[0] = quad;
            var source = quads.Slice();
            var output = vertices.Slice();
            var triangles = indices.Slice();
            ParticleQuadMesh.Fill(ref source, ref output, ref triangles);
            ParticleQuadMesh.FixWinding(ref output, ref triangles);
            for (var i = 0; i < 4; i++)
            {
                var match = Array.IndexOf(uv, vertices[i].uv);
                Assert.That(match, Is.GreaterThanOrEqualTo(0));
                var point = (float2)geometry[match] * new float2(8, -8) * flip;
                var rotated = new float2(-point.y, point.x);
                var expected = origin + math.mul(birthBasis, rotated);
                Assert.That(math.distance(((float3)vertices[i].position).xy, expected), Is.LessThan(0.00001f));
            }

            var a = vertices[indices[1]].position - vertices[indices[0]].position;
            var b = vertices[indices[2]].position - vertices[indices[0]].position;
            Assert.That((a.x * b.y) - (a.y * b.x), Is.GreaterThan(0));
        }

        [Test]
        public void NonrectangularSpriteGeometryIsRejected()
        {
            var geometry = new[] { Vector2.zero, Vector2.right, Vector2.up, new Vector2(0.75f, 1) };
            var uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            Assert.Throws<ArgumentException>(() => UIParticleSpriteUtility.Compile(geometry, uv, new ushort[] { 0, 2, 1, 1, 2, 3 }, 100, Vector2.one, new float2(1)));
        }

        [Test]
        public void TightTriangleAndCrossedUvMappingsAreRejected()
        {
            var geometry = new[] { Vector2.zero, Vector2.right, Vector2.up };
            Assert.Throws<ArgumentException>(() => UIParticleSpriteUtility.Compile(geometry, geometry, new ushort[] { 0, 2, 1 }, 100, Vector2.one, new float2(1)));
            geometry = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            var crossed = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            Assert.Throws<ArgumentException>(() => UIParticleSpriteUtility.Compile(geometry, crossed, new ushort[] { 0, 2, 1, 1, 2, 3 }, 100, Vector2.one, new float2(1)));
        }
    }
}
