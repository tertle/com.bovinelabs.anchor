namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ParticleQuadMeshTests
    {
        [Test]
        public void FillRotatesNonuniformQuadAndPreservesTintAndTextureRect()
        {
            var quads = new NativeArray<ParticleQuad>(1, Allocator.Temp);
            var vertices = new NativeArray<Vertex>(4, Allocator.Temp);
            var indices = new NativeArray<ushort>(6, Allocator.Temp);
            var tint = new Color32(193, 71, 29, 0);
            quads[0] = new ParticleQuad
            {
                Position = new float2(10, 20),
                Size = new float2(8, 4),
                Angle = math.PI / 2,
                Tint = tint,
                Uv = new float4(0.1f, 0.2f, 0.7f, 0.8f),
            };
            var source = quads.Slice();
            var output = vertices.Slice();
            var triangles = indices.Slice();
            ParticleQuadMesh.Fill(ref source, ref output, ref triangles);

            var expected = new[] { new Vector2(12, 16), new Vector2(12, 24), new Vector2(8, 24), new Vector2(8, 16) };
            var uv = new[] { new Vector2(0.1f, 0.8f), new Vector2(0.7f, 0.8f), new Vector2(0.7f, 0.2f), new Vector2(0.1f, 0.2f) };
            for (var i = 0; i < 4; i++)
            {
                Assert.That(Vector2.Distance(vertices[i].position, expected[i]), Is.LessThan(0.00001f));
                Assert.That(vertices[i].position.z, Is.EqualTo(Vertex.nearZ));
                Assert.That(vertices[i].tint, Is.EqualTo(tint));
                Assert.That(vertices[i].uv, Is.EqualTo(uv[i]));
            }

            Assert.That(indices.ToArray(), Is.EqualTo(new ushort[] { 0, 1, 2, 0, 2, 3 }));
            var a = (Vector2)vertices[1].position - (Vector2)vertices[0].position;
            var b = (Vector2)vertices[2].position - (Vector2)vertices[0].position;
            Assert.That((a.x * b.y) - (a.y * b.x), Is.GreaterThan(0), "Clockwise in UI's downward Y coordinates");
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(16382, 1)]
        [TestCase(16383, 1)]
        [TestCase(16384, 2)]
        [TestCase(20000, 2)]
        public void ChunkedFillPreservesEveryQuadWithoutIndexWrap(int count, int expectedChunks)
        {
            var quads = new NativeArray<ParticleQuad>(count, Allocator.Temp);
            for (var i = 0; i < count; i++)
            {
                quads[i] = new ParticleQuad
                {
                    Position = new float2(i, 3),
                    Size = new float2(2, 2),
                    Tint = new Color32(255, 255, 255, 255),
                    Uv = new float4(0, 0, 1, 1),
                };
            }

            var chunks = 0;
            for (var offset = 0; offset < count;)
            {
                var chunk = ParticleQuadMesh.GetChunkSize(count - offset);
                var vertices = new NativeArray<Vertex>(checked(chunk * 4), Allocator.Temp);
                var indices = new NativeArray<ushort>(checked(chunk * 6), Allocator.Temp);
                var source = quads.Slice(offset, chunk);
                var output = vertices.Slice();
                var triangles = indices.Slice();
                ParticleQuadMesh.Fill(ref source, ref output, ref triangles);
                Assert.That(vertices.Length, Is.LessThanOrEqualTo(ushort.MaxValue));
                for (var q = 0; q < chunk; q++)
                {
                    Assert.That(vertices[q * 4].position.x, Is.EqualTo(offset + q - 1));
                    Assert.That(indices[(q * 6) + 5], Is.EqualTo((q * 4) + 3));
                }

                offset += chunk;
                chunks++;
            }

            Assert.That(chunks, Is.EqualTo(expectedChunks));
        }
    }
}
