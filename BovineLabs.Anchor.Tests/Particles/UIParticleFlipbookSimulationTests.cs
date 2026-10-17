namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;

    public class UIParticleFlipbookSimulationTests
    {
        private UIParticleCompiledData _data;
        private NativeArray<UIParticle> _particles;
        private NativeArray<UIParticleEmitterState> _states;
        private NativeArray<UIParticleEmissionStream> _streams;
        private NativeArray<int2> _flipbookStates;
        private NativeArray<ParticleQuad> _quads;
        private NativeArray<int> _counts;
        private float4x4 _transform;

        [SetUp]
        public void SetUp()
        {
            _data = new UIParticleCompiledData
            {
                Emitters = new NativeArray<UIParticleEmitter>(2, Allocator.Persistent),
                Bursts = new NativeArray<UIParticleBurst>(1, Allocator.Persistent),
                Samples = new NativeArray<UIParticleSample>(2 * UIParticleCompiledEffect.SampleCount, Allocator.Persistent),
                Steps = new NativeArray<UIParticleColorStep>(0, Allocator.Persistent),
                Flipbooks = new NativeArray<UIParticleFlipbook>(2, Allocator.Persistent),
            };
            _data.Emitters[0] = new UIParticleEmitter
            {
                Capacity = 2,
                Duration = 1,
                Uv = new float4(0, 0, 1, 1),
            };
            _data.Emitters[1] = new UIParticleEmitter
            {
                Offset = 2,
                Capacity = 4,
                Duration = 1,
                BurstCount = 1,
                Lifetime = new float2(1, 2),
                Speed = new float2(2, 4),
                Size = new float2(3, 5),
                Spread = math.PI * 2,
                ColorMin = new float4(0.5f),
                ColorMax = new float4(1),
                Uv = new float4(0, 0, 1, 1),
            };
            _data.Bursts[0] = new UIParticleBurst {
                Count = 4
            };
            _data.Flipbooks[1] = UIParticleFlipbookUtility.Compile(new UIParticleFlipbookSettings
            {
                Columns = 4,
                Rows = 2,
                Cycles = 0,
            }, new float4(0, 0, 1, 1), new int2(128), 0);
            for (var i = 0; i < _data.Samples.Length; i++)
            {
                _data.Samples[i] = new UIParticleSample
                {
                    Size = 1,
                    Color = new float4(1),
                };
            }

            _particles = new NativeArray<UIParticle>(6, Allocator.Persistent);
            _states = new NativeArray<UIParticleEmitterState>(2, Allocator.Persistent);
            _streams = new NativeArray<UIParticleEmissionStream>(3, Allocator.Persistent);
            _flipbookStates = new NativeArray<int2>(4, Allocator.Persistent);
            _quads = new NativeArray<ParticleQuad>(6, Allocator.Persistent);
            _counts = new NativeArray<int>(2, Allocator.Persistent);
            _transform = float4x4.identity;
        }

        [TearDown]
        public void TearDown()
        {
            _data.Emitters.Dispose();
            _data.Bursts.Dispose();
            _data.Samples.Dispose();
            _data.Steps.Dispose();
            _data.Flipbooks.Dispose();
            _particles.Dispose();
            _states.Dispose();
            _streams.Dispose();
            _flipbookStates.Dispose();
            _quads.Dispose();
            _counts.Dispose();
        }

        [Test]
        public void StableCompactionKeepsFramesWithTheirParticlesInMixedEffects()
        {
            for (var i = 0; i < 5; i++)
            {
                _particles[i] = new UIParticle
                {
                    Position = new float2(i * 10, 0),
                    Age = i == 3 ? 0.9f : 0.1f,
                    Lifetime = 1,
                    Size = 8,
                    Tint = new float4(1),
                    Basis = float2x2.identity,
                };
            }

            _states[0] = new UIParticleEmitterState {
                Count = 2
            };
            _states[1] = new UIParticleEmitterState {
                Count = 3
            };
            _flipbookStates[0] = new int2(3, 0);
            _flipbookStates[1] = new int2(5, 0);
            _flipbookStates[2] = new int2(7, 0);
            UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbookStates, 0.2, 7, false, ref _transform);
            Assert.That(_states[0].Count, Is.EqualTo(2));
            Assert.That(_states[1].Count, Is.EqualTo(2));
            Assert.That(_particles[2].Position.x, Is.EqualTo(20));
            Assert.That(_particles[3].Position.x, Is.EqualTo(40));
            Assert.That(_flipbookStates[0].x, Is.EqualTo(3));
            Assert.That(_flipbookStates[1].x, Is.EqualTo(7));

            var tint = new float4(1);
            UIParticleMesh.Fill(ref _data, ref _particles, ref _states, ref _flipbookStates, ref _quads, ref _counts, ref tint);
            Assert.That(_quads[0].Uv, Is.EqualTo(new float4(0, 0, 1, 1)));
            Assert.That(_quads[2].Uv, Is.EqualTo(new float4(0.75f, 0.5f, 1, 1)));
            Assert.That(_quads[3].Uv, Is.EqualTo(new float4(0.75f, 0, 1, 0.5f)));
        }

        [Test]
        public void BasicAndOptionalPathsPreserveAppearanceAndSkipZeroSizeParticles()
        {
            _data.Flipbooks[1] = default;
            _states[0] = new UIParticleEmitterState
            {
                Count = 2,
            };
            _particles[0] = new UIParticle
            {
                Position = new float2(2, 3),
                Age = 0.5f,
                Lifetime = 1,
                Size = 8,
                Rotation = 0.2f,
                Tint = new float4(0.5f, 0.5f, 0.5f, 1),
                Basis = float2x2.identity,
            };
            _particles[1] = new UIParticle
            {
                Age = 0.5f,
                Lifetime = 1,
            };
            var tint = new float4(0.5f, 1, 1, 1);
            UIParticleMesh.Fill(ref _data, ref _particles, ref _states, ref _flipbookStates, ref _quads, ref _counts, ref tint);
            var optional = _quads[0];
            Assert.That(_counts[0], Is.EqualTo(1));
            Assert.That(optional.Position, Is.EqualTo(new float2(2, 3)));
            Assert.That(optional.Size, Is.EqualTo(new float2(8)));
            Assert.That(optional.Tint, Is.EqualTo(new Color32(64, 128, 128, 255)));
            var descriptors = _data.Flipbooks;
            try
            {
                _data.Flipbooks = default;
                UIParticleMesh.Fill(ref _data, ref _particles, ref _states, ref _flipbookStates, ref _quads, ref _counts, ref tint);
                Assert.That(_counts[0], Is.EqualTo(1));
                Assert.That(_quads[0], Is.EqualTo(optional));
                Assert.That(_states[0].Count, Is.EqualTo(2));
            }
            finally
            {
                _data.Flipbooks = descriptors;
            }
        }

        [Test]
        public void EnablingFlipbooksPreservesBirthParametersAndReplayingOverwritesOldFrames()
        {
            var flipbook = _data.Flipbooks[1];
            flipbook.StartFrame = new int2(0, 7);
            _data.Flipbooks[1] = default;
            UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbookStates, 0, 33, true, ref _transform);
            var baseline = _particles.ToArray();
            _data.Flipbooks[1] = flipbook;
            int2[] firstFrames = null;
            for (var replay = 0; replay < 2; replay++)
            {
                _states[1] = default;
                _streams[1] = default;
                _streams[2] = default;
                for (var i = 0; i < _flipbookStates.Length; i++)
                {
                    _flipbookStates[i] = new int2(-1);
                }

                UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbookStates, 0, 33, true, ref _transform);
                Assert.That(_states[1].Count, Is.EqualTo(4));
                for (var i = 0; i < 4; i++)
                {
                    Assert.That(_particles[i + 2], Is.EqualTo(baseline[i + 2]));
                    Assert.That(_flipbookStates[i].x, Is.InRange(0, 7));
                    if (firstFrames != null)
                    {
                        Assert.That(_flipbookStates[i], Is.EqualTo(firstFrames[i]));
                    }
                }

                firstFrames = _flipbookStates.ToArray();
            }
        }
    }
}
