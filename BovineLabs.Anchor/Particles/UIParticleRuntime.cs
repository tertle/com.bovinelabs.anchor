namespace BovineLabs.Anchor.Particles
{
    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;
    using Unity.Profiling;
    using UnityEngine.UIElements;

    public sealed class UIParticleRuntime : IDisposable
    {
        private static readonly ProfilerMarker SimulationMarker = new("Anchor.Particles.Simulation");
        private static readonly ProfilerMarker AppearanceMarker = new("Anchor.Particles.Appearance");
        internal readonly UIParticleCompiledEffect Compiled;
        internal NativeArray<UIParticle> Particles;
        internal NativeArray<UIParticleEmitterState> States;
        internal NativeArray<ParticleQuad> Quads;
        internal NativeArray<int> QuadCounts;
        private NativeArray<UIParticleEmissionStream> _streams;
        private uint _seed;
        private float _emissionScale = 1;
        private bool _emitting;
        private bool _disposed;

        public UIParticleRuntime(UIParticleEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            Compiled = effect.Acquire();
            Particles = new NativeArray<UIParticle>(Compiled.Capacity, Allocator.Persistent);
            States = new NativeArray<UIParticleEmitterState>(Compiled.Data.Emitters.Length, Allocator.Persistent);
            Quads = new NativeArray<ParticleQuad>(Compiled.Capacity, Allocator.Persistent);
            QuadCounts = new NativeArray<int>(States.Length, Allocator.Persistent);
            _streams = new NativeArray<UIParticleEmissionStream>(Compiled.Data.Bursts.Length + States.Length, Allocator.Persistent);
        }

        public static int ParticleStride => UnsafeUtility.SizeOf<UIParticle>();
        public int Capacity => Compiled.Capacity;
        public long ParticleCapacityBytes => (long)Capacity * ParticleStride;
        public long NativeBytes => ParticleCapacityBytes + ((long)Quads.Length * UnsafeUtility.SizeOf<ParticleQuad>()) +
            ((long)States.Length * (UnsafeUtility.SizeOf<UIParticleEmitterState>() + sizeof(int))) +
            ((long)_streams.Length * UnsafeUtility.SizeOf<UIParticleEmissionStream>());
        public long SharedCompiledBytes => Compiled.NativeBytes;
        public uint Revision => Compiled.Revision;
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        public float EmissionScale
        {
            get => _emissionScale;
            set
            {
                if (!float.IsFinite(value) || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _emissionScale = value;
            }
        }

        public UIParticleCounters Counters
        {
            get
            {
                ThrowIfDisposed();
                var counters = new UIParticleCounters();
                for (var i = 0; i < States.Length; i++)
                {
                    var state = States[i];
                    counters.Attempted += Math.Min(state.Attempted, ulong.MaxValue - counters.Attempted);
                    counters.Emitted += Math.Min(state.Emitted, ulong.MaxValue - counters.Emitted);
                    counters.Dropped += Math.Min(state.Dropped, ulong.MaxValue - counters.Dropped);
                }

                return counters;
            }
        }

        public ulong DroppedCount
        {
            get
            {
                ThrowIfDisposed();
                ulong count = 0;
                for (var i = 0; i < States.Length; i++)
                {
                    var dropped = States[i].Dropped;
                    count = dropped > ulong.MaxValue - count ? ulong.MaxValue : count + dropped;
                }

                return count;
            }
        }

        public int LiveCount
        {
            get
            {
                ThrowIfDisposed();
                var count = 0;
                for (var i = 0; i < States.Length; i++)
                {
                    count += States[i].Count;
                }

                return count;
            }
        }

        public void Play(uint seed = 1)
        {
            Clear();
            _seed = seed;
            _emitting = true;
            IsPlaying = States.Length != 0;
        }

        public void StopEmitting()
        {
            ThrowIfDisposed();
            _emitting = false;
            UpdatePlaying();
        }

        public void Pause()
        {
            ThrowIfDisposed();
            IsPaused = IsPlaying;
        }

        public void Resume()
        {
            ThrowIfDisposed();
            IsPaused = false;
        }

        public void Clear()
        {
            ThrowIfDisposed();
            for (var e = 0; e < States.Length; e++)
            {
                States[e] = default;
                QuadCounts[e] = 0;
            }

            for (var s = 0; s < _streams.Length; s++)
            {
                _streams[s] = default;
            }

            IsPlaying = false;
            IsPaused = false;
            _emitting = false;
        }

        public bool Advance(double elapsed)
        {
            var identity = float4x4.identity;
            return Advance(elapsed, ref identity);
        }

        internal bool Advance(double elapsed, ref float4x4 spawnTransform)
        {
            ThrowIfDisposed();
            if (!math.isfinite(elapsed) || elapsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            }

            if (!IsPlaying || IsPaused)
            {
                return false;
            }

            using (SimulationMarker.Auto())
            {
                var remaining = math.min(elapsed, 4d / 60);
                var data = Compiled.Data;
                for (var step = 0; step < 4; step++)
                {
                    var delta = math.min(remaining, 1d / 60);
                    UIParticleSimulation.Step(ref data, ref Particles, ref States, ref _streams, delta, _seed,
                        _emitting, ref spawnTransform, _emissionScale);
                    remaining -= delta;
                    if (remaining <= 0)
                    {
                        break;
                    }
                }
            }

            UpdatePlaying();
            return true;
        }

        internal void PrepareMesh(float4 tint)
        {
            ThrowIfDisposed();
            using (AppearanceMarker.Auto())
            {
                var data = Compiled.Data;
                UIParticleMesh.Fill(ref data, ref Particles, ref States, ref Quads, ref QuadCounts, ref tint);
            }
        }

        internal void Draw(MeshGenerationContext context, ref float4x4 simulationToLocal, bool panelSpace)
        {
            var offset = 0;
            for (var e = 0; e < QuadCounts.Length;)
            {
                var texture = Compiled.Textures[e];
                var count = QuadCounts[e++];
                while (e < QuadCounts.Length && Compiled.Textures[e] == texture)
                {
                    count = checked(count + QuadCounts[e++]);
                }

                var quads = Quads.Slice(offset, count);
                ParticleQuadMesh.Draw(context, quads, texture, simulationToLocal, panelSpace);
                offset += count;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Particles.Dispose();
            States.Dispose();
            Quads.Dispose();
            QuadCounts.Dispose();
            _streams.Dispose();
            Compiled.Release();
            _disposed = true;
            IsPlaying = false;
            IsPaused = false;
        }

        private void UpdatePlaying()
        {
            var active = false;
            for (var e = 0; e < States.Length; e++)
            {
                var state = States[e];
                var settings = Compiled.Data.Emitters[e];
                active |= state.Count != 0 || (_emitting && (settings.Looping || state.Time < settings.Delay + settings.Duration));
            }

            IsPlaying = active;
            if (!active)
            {
                IsPaused = false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UIParticleRuntime));
            }
        }
    }
}
