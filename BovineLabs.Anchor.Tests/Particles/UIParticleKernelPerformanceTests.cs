namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;

    public class UIParticleKernelPerformanceTests
    {
        private UIParticleCompiledData _data;
        private NativeArray<UIParticle> _particles;
        private NativeArray<UIParticleEmitterState> _states;
        private NativeArray<UIParticleEmissionStream> _streams;
        private NativeArray<int2> _flipbookStates;
        private UIParticleFlights _flights;
        private NativeArray<ParticleQuad> _quads;
        private NativeArray<int> _counts;
        private float4x4 _transform;
        private float4 _tint;
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
            _data.Emitters.Dispose();
            _data.Bursts.Dispose();
            _data.Samples.Dispose();
            _data.Steps.Dispose();
            _particles.Dispose();
            _states.Dispose();
            _streams.Dispose();
            _quads.Dispose();
            _counts.Dispose();
            if (_data.Flipbooks.IsCreated)
            {
                _data.Flipbooks.Dispose();
                _flipbookStates.Dispose();
            }

            BurstCompiler.Options.EnableBurstCompilation = _previousBurst;
            BurstCompiler.Options.EnableBurstCompileSynchronously = _previousSynchronous;
        }

        [TestCase(64), TestCase(100), TestCase(1000), TestCase(5000), Performance]
        public void BasicUpdateMaintainsLivePopulation(int count)
        {
            Prepare(count);
            MeasureUpdate(count);
        }

        [TestCase(64), TestCase(100), TestCase(1000), TestCase(5000), Performance]
        public void FlipbookUpdateMaintainsLivePopulation(int count)
        {
            Prepare(count);
            var flipbook = UIParticleFlipbookUtility.Compile(new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 4,
                Cycles = 2,
            }, new float4(0, 0, 1, 1), new int2(256), 0);
            _data.Flipbooks = new NativeArray<UIParticleFlipbook>(1, Allocator.Persistent);
            _data.Flipbooks[0] = flipbook;
            _flipbookStates = new NativeArray<int2>(count, Allocator.Persistent);
            for (var i = 0; i < count; i++)
            {
                _flipbookStates[i] = UIParticleFlipbookUtility.Sample(in flipbook, 7, 0, (uint)i);
            }

            MeasureUpdate(count);
        }

        private void Prepare(int count)
        {
            _data = new UIParticleCompiledData
            {
                Emitters = new NativeArray<UIParticleEmitter>(1, Allocator.Persistent),
                Bursts = new NativeArray<UIParticleBurst>(0, Allocator.Persistent),
                Samples = new NativeArray<UIParticleSample>(UIParticleCompiledEffect.SampleCount, Allocator.Persistent),
                Steps = new NativeArray<UIParticleColorStep>(0, Allocator.Persistent),
            };
            _data.Emitters[0] = new UIParticleEmitter
            {
                Capacity = count,
                Duration = 1,
                Acceleration = new float2(0, 2),
                Uv = new float4(0, 0, 1, 1),
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
            _quads = new NativeArray<ParticleQuad>(count, Allocator.Persistent);
            _counts = new NativeArray<int>(1, Allocator.Persistent);
            _states[0] = new UIParticleEmitterState {
                Count = count
            };
            for (var i = 0; i < count; i++)
            {
                _particles[i] = new UIParticle
                {
                    Position = new float2(i % 100, i / 100),
                    Velocity = new float2(3, 5),
                    Lifetime = 1000,
                    Size = 8,
                    Tint = new float4(1),
                    Basis = float2x2.identity,
                };
            }

            _transform = float4x4.identity;
            _tint = new float4(1);
        }

        private void MeasureUpdate(int count)
        {
            UpdateParticles();
            Measure.Method(UpdateParticles).SampleGroup(new SampleGroup("SimulationAndAppearance", SampleUnit.Microsecond))
                .WarmupCount(10).MeasurementCount(30).IterationsPerMeasurement(1).GC().Run();
            Assert.That(_states[0].Count, Is.EqualTo(count));
            Assert.That(_counts[0], Is.EqualTo(count));
        }

        private void UpdateParticles()
        {
            UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbookStates, ref _flights, 1d / 60, 7, false, ref _transform);
            UIParticleMesh.Fill(ref _data, ref _particles, ref _states, ref _flipbookStates, ref _quads, ref _counts, ref _tint);
        }
    }
}
