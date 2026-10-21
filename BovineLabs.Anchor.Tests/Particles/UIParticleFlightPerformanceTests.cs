namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine.UIElements;

    public class UIParticleFlightPerformanceTests
    {
        private UIParticleCompiledData _data;
        private NativeArray<UIParticle> _particles;
        private NativeArray<UIParticleEmitterState> _states;
        private NativeArray<UIParticleEmissionStream> _streams;
        private NativeArray<int2> _flipbooks;
        private UIParticleFlights _flights;
        private NativeArray<ParticleQuad> _quads;
        private NativeArray<int> _counts;
        private NativeArray<Vertex> _vertices;
        private NativeArray<ushort> _indices;
        private float4x4 _transform;
        private float4 _tint;
        private bool _previousBurst;
        private bool _previousSynchronous;

        [OneTimeSetUp]
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
            _data.Emitters.Dispose();
            _data.Bursts.Dispose();
            _data.Samples.Dispose();
            _data.Steps.Dispose();
            _data.Flights.Dispose();
            _particles.Dispose();
            _states.Dispose();
            _streams.Dispose();
            _flights.States.Dispose();
            _quads.Dispose();
            _counts.Dispose();
            _vertices.Dispose();
            _indices.Dispose();
        }

        [OneTimeTearDown]
        public void RestoreBurst()
        {
            BurstCompiler.Options.EnableBurstCompilation = _previousBurst;
            BurstCompiler.Options.EnableBurstCompileSynchronously = _previousSynchronous;
        }

        [TestCase(5000), Performance]
        public void FollowFlightSeparatesSimulationAppearanceAndMeshCosts(int count)
        {
            Prepare(count);
            Simulate();
            Appearance();
            Mesh();
            Measure.Method(Simulate).SampleGroup(new SampleGroup("FlightSimulation", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Measure.Method(Appearance).SampleGroup(new SampleGroup("FlightAppearance", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Measure.Method(Mesh).SampleGroup(new SampleGroup("FlightMesh", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Measure.Custom(new SampleGroup("FlightStateBytes", SampleUnit.Byte), (long)count * UnsafeUtility.SizeOf<UIParticleFlightState>());
            Measure.Custom(new SampleGroup("OrdinaryParticleStride", SampleUnit.Byte), UnsafeUtility.SizeOf<UIParticle>());
            Assert.That(_states[0].Count, Is.EqualTo(count));
            Assert.That(_counts[0], Is.EqualTo(count));
            Assert.That(_flights.Arrivals, Is.Zero);
        }

        [TestCase(5000), Performance]
        public void ManySourceRequestsAdmitOnlyAvailableCapacity(int count)
        {
            Prepare(count);
            void EmitSources()
            {
                _states[0] = default;
                for (var request = 0; request < 100; request++)
                {
                    _transform.c3.x = request * 3;
                    UIParticleSimulation.Emit(ref _data, ref _particles, ref _states, ref _flipbooks, ref _flights, 0, count / 50, 7, 1,
                        ref _transform, out _);
                }
            }

            EmitSources();
            Measure.Method(EmitSources).SampleGroup(new SampleGroup("100SourceRequests", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Assert.That(_states[0].Count, Is.EqualTo(count));
            Assert.That(_states[0].Attempted, Is.EqualTo(count * 2));
            Assert.That(_states[0].Dropped, Is.EqualTo(count));
        }

        private void Prepare(int count)
        {
            _data = new UIParticleCompiledData
            {
                Emitters = new NativeArray<UIParticleEmitter>(1, Allocator.Persistent),
                Bursts = new NativeArray<UIParticleBurst>(0, Allocator.Persistent),
                Samples = new NativeArray<UIParticleSample>(UIParticleCompiledEffect.SampleCount, Allocator.Persistent),
                Steps = new NativeArray<UIParticleColorStep>(0, Allocator.Persistent),
                Flights = new NativeArray<UIParticleFlight>(1, Allocator.Persistent),
            };
            _data.Emitters[0] = new UIParticleEmitter
            {
                Capacity = count,
                Duration = 1,
                Lifetime = new float2(1),
                Size = new float2(8),
                ColorMin = new float4(1),
                ColorMax = new float4(1),
                Uv = new float4(0, 0, 1, 1),
            };
            _data.Flights[0] = new UIParticleFlight
            {
                Enabled = true,
                FlightDuration = new float2(1000),
                PathBend = 40,
                TargetMode = UIParticleTargetMode.Follow,
            };
            for (var i = 0; i < _data.Samples.Length; i++)
            {
                _data.Samples[i] = new UIParticleSample
                {
                    Size = 1,
                    Color = new float4(1),
                };
            }

            _particles = new NativeArray<UIParticle>(count, Allocator.Persistent);
            _states = new NativeArray<UIParticleEmitterState>(1, Allocator.Persistent);
            _streams = new NativeArray<UIParticleEmissionStream>(1, Allocator.Persistent);
            _flights = new UIParticleFlights
            {
                States = new NativeArray<UIParticleFlightState>(count, Allocator.Persistent),
                Target = new float2(800, 400),
            };
            _quads = new NativeArray<ParticleQuad>(count, Allocator.Persistent);
            _counts = new NativeArray<int>(1, Allocator.Persistent);
            _vertices = new NativeArray<Vertex>(count * 4, Allocator.Persistent);
            _indices = new NativeArray<ushort>(count * 6, Allocator.Persistent);
            _transform = float4x4.identity;
            _tint = new float4(1);
            UIParticleSimulation.Emit(ref _data, ref _particles, ref _states, ref _flipbooks, ref _flights, 0, count, 7, 1,
                ref _transform, out _);
        }

        private void Simulate()
        {
            _flights.Target.x += 0.1f;
            UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbooks, ref _flights, 1d / 60, 7, false, ref _transform);
        }

        private void Appearance() => UIParticleMesh.Fill(ref _data, ref _particles, ref _states, ref _flipbooks, ref _quads, ref _counts, ref _tint);

        private void Mesh()
        {
            var source = _quads.Slice();
            var output = _vertices.Slice();
            var indices = _indices.Slice();
            ParticleQuadMesh.Fill(ref source, ref output, ref indices);
        }
    }
}
