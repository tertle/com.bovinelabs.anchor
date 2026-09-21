namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ParticleQuadPerformanceTests
    {
        private bool _previousBurst;
        private bool _previousSynchronous;

        [SetUp]
        public void SetUp()
        {
            _previousBurst = BurstCompiler.Options.EnableBurstCompilation;
            _previousSynchronous = BurstCompiler.Options.EnableBurstCompileSynchronously;
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
        }

        [TearDown]
        public void TearDown()
        {
            BurstCompiler.Options.EnableBurstCompileSynchronously = _previousSynchronous;
            BurstCompiler.Options.EnableBurstCompilation = _previousBurst;
        }

        [TestCase(64)]
        [TestCase(256)]
        [TestCase(1000)]
        [TestCase(5000)]
        [TestCase(20000)]
        [Performance]
        public void MeshFill(int count)
        {
            Assert.That(BurstCompiler.IsEnabled, Is.True, "The native mesh-fill baseline requires Burst to be enabled.");
            using var quads = new NativeArray<ParticleQuad>(count, Allocator.Persistent);
            using var vertices = new NativeArray<Vertex>(count * 4, Allocator.Persistent);
            using var indices = new NativeArray<ushort>(count * 6, Allocator.Persistent);
            var input = quads.Slice();
            for (var q = 0; q < count; q++)
            {
                input[q] = new ParticleQuad
                {
                    Position = new float2(q % 100, q / 100),
                    Size = new float2(8, 12),
                    Angle = q * 0.01f,
                    Tint = new Color32(255, 128, 64, 128),
                    Uv = new float4(0, 0, 1, 1),
                };
            }

            void Fill()
            {
                for (var offset = 0; offset < count;)
                {
                    var chunk = ParticleQuadMesh.GetChunkSize(count - offset);
                    var source = quads.Slice(offset, chunk);
                    var output = vertices.Slice(offset * 4, chunk * 4);
                    var triangles = indices.Slice(offset * 6, chunk * 6);
                    ParticleQuadMesh.Fill(ref source, ref output, ref triangles);
                    offset += chunk;
                }
            }

            Fill();
            Measure.Method(Fill).SampleGroup(new SampleGroup("MeshFill", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Measure.Custom(new SampleGroup("LiveQuads", SampleUnit.Undefined), count);
            Measure.Custom(new SampleGroup("VertexBytes", SampleUnit.Byte), count * 4L * UnsafeUtility.SizeOf<Vertex>());
            Measure.Custom(new SampleGroup("IndexBytes", SampleUnit.Byte), count * 6L * sizeof(ushort));
            Measure.Custom(new SampleGroup("InputBytes", SampleUnit.Byte), count * (long)UnsafeUtility.SizeOf<ParticleQuad>());
            Assert.That(vertices[(count * 4) - 1].tint.a, Is.EqualTo(128));
        }
    }
}
