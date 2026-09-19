namespace BovineLabs.Anchor.Particles
{
    using System;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.UIElements;

    [BurstCompile]
    public static class ParticleQuadMesh
    {
        // AllocateTempMesh permits 65,535 vertices; leave an incomplete quad unused.
        public const int QuadsPerMesh = ushort.MaxValue / 4;

        private static readonly ProfilerMarker FillMarker = new("Anchor.Particles.MeshFill");
        private static readonly ProfilerMarker AllocateMarker = new("Anchor.Particles.MeshAllocate");
        private static readonly ProfilerMarker SubmitMarker = new("Anchor.Particles.MeshSubmit");

        public static int GetChunkSize(int remaining)
        {
            if (remaining < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remaining));
            }

            return Math.Min(remaining, QuadsPerMesh);
        }

        public static void Draw(MeshGenerationContext context, NativeSlice<ParticleQuad> quads, Texture texture = null)
        {
            Draw(context, quads, texture, float4x4.identity, false);
        }

        internal static void Draw(MeshGenerationContext context, NativeSlice<ParticleQuad> quads, Texture texture, float4x4 transform, bool transformPositions)
        {
            for (var offset = 0; offset < quads.Length;)
            {
                var count = GetChunkSize(quads.Length - offset);
                NativeSlice<Vertex> vertices;
                NativeSlice<ushort> indices;
                using (AllocateMarker.Auto())
                {
                    context.AllocateTempMesh(checked(count * 4), checked(count * 6), out vertices, out indices);
                }

                var source = quads.Slice(offset, count);
                using (FillMarker.Auto())
                {
                    Fill(ref source, ref vertices, ref indices);
                    if (transformPositions)
                    {
                        Transform(ref vertices, ref transform);
                    }
                }

                using (SubmitMarker.Auto())
                {
                    context.DrawMesh(vertices, indices, texture);
                }

                // The renderer consumes these slices later. Only Unity owns/recycles their storage.
                offset += count;
            }
        }

        [BurstCompile]
        internal static void Transform(ref NativeSlice<Vertex> vertices, ref float4x4 transform)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var point = math.transform(transform, new float3(vertex.position.x, vertex.position.y, 0));
                vertex.position = new Vector3(point.x, point.y, Vertex.nearZ);
                vertices[i] = vertex;
            }
        }

        // One chunk, with exactly four vertices and six indices per quad. Callers own sizing and lifetime.
        [BurstCompile]
        public static void Fill(ref NativeSlice<ParticleQuad> quads, ref NativeSlice<Vertex> vertices, ref NativeSlice<ushort> indices)
        {
            for (var q = 0; q < quads.Length; q++)
            {
                var quad = quads[q];
                math.sincos(quad.Angle, out var sin, out var cos);
                var x = new float2(cos, sin) * quad.Size.x * 0.5f;
                var y = new float2(-sin, cos) * quad.Size.y * 0.5f;
                var v = q * 4;
                for (var corner = 0; corner < 4; corner++)
                {
                    var right = corner == 1 || corner == 2;
                    var bottom = corner >= 2;
                    var p = quad.Position + (right ? x : -x) + (bottom ? y : -y);
                    vertices[v + corner] = new Vertex
                    {
                        position = new Vector3(p.x, p.y, Vertex.nearZ),
                        tint = quad.Tint,
                        uv = new Vector2(right ? quad.Uv.z : quad.Uv.x, bottom ? quad.Uv.y : quad.Uv.w),
                    };
                }

                var i = q * 6;
                indices[i] = (ushort)v;
                indices[i + 1] = (ushort)(v + 1);
                indices[i + 2] = (ushort)(v + 2);
                indices[i + 3] = (ushort)v;
                indices[i + 4] = (ushort)(v + 2);
                indices[i + 5] = (ushort)(v + 3);
            }
        }
    }
}
