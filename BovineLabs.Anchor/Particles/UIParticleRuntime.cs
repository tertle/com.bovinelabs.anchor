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
        private NativeArray<UIParticleEmissionStream> streams;
        private uint seed;
        private bool emitting;
        private bool disposed;

        public UIParticleRuntime(UIParticleEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            this.Compiled = effect.Acquire();
            this.Particles = new NativeArray<UIParticle>(this.Compiled.Capacity, Allocator.Persistent);
            this.States = new NativeArray<UIParticleEmitterState>(this.Compiled.Data.Emitters.Length, Allocator.Persistent);
            this.Quads = new NativeArray<ParticleQuad>(this.Compiled.Capacity, Allocator.Persistent);
            this.QuadCounts = new NativeArray<int>(this.States.Length, Allocator.Persistent);
            this.streams = new NativeArray<UIParticleEmissionStream>(this.Compiled.Data.Bursts.Length + this.States.Length, Allocator.Persistent);
        }

        public static int ParticleStride => UnsafeUtility.SizeOf<UIParticle>();
        public int Capacity => this.Compiled.Capacity;
        public long ParticleCapacityBytes => (long)this.Capacity * ParticleStride;
        public long NativeBytes => this.ParticleCapacityBytes + ((long)this.Quads.Length * UnsafeUtility.SizeOf<ParticleQuad>()) +
            ((long)this.States.Length * (UnsafeUtility.SizeOf<UIParticleEmitterState>() + sizeof(int))) +
            ((long)this.streams.Length * UnsafeUtility.SizeOf<UIParticleEmissionStream>());
        public long SharedCompiledBytes => this.Compiled.NativeBytes;
        public uint Revision => this.Compiled.Revision;
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        public int LiveCount
        {
            get
            {
                this.ThrowIfDisposed();
                var count = 0;
                for (var i = 0; i < this.States.Length; i++)
                {
                    count += this.States[i].Count;
                }

                return count;
            }
        }

        public void Play(uint seed = 1)
        {
            this.Clear();
            this.seed = seed;
            this.emitting = true;
            this.IsPlaying = this.States.Length != 0;
        }

        public void StopEmitting()
        {
            this.ThrowIfDisposed();
            this.emitting = false;
            this.UpdatePlaying();
        }

        public void Pause()
        {
            this.ThrowIfDisposed();
            this.IsPaused = this.IsPlaying;
        }

        public void Resume()
        {
            this.ThrowIfDisposed();
            this.IsPaused = false;
        }

        public void Clear()
        {
            this.ThrowIfDisposed();
            for (var e = 0; e < this.States.Length; e++)
            {
                this.States[e] = default;
                this.QuadCounts[e] = 0;
            }

            this.IsPlaying = false;
            this.IsPaused = false;
            this.emitting = false;
        }

        public bool Advance(double elapsed)
        {
            var identity = float4x4.identity;
            return this.Advance(elapsed, ref identity);
        }

        internal bool Advance(double elapsed, ref float4x4 spawnTransform)
        {
            this.ThrowIfDisposed();
            if (!math.isfinite(elapsed) || elapsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            }

            if (!this.IsPlaying || this.IsPaused)
            {
                return false;
            }

            using (SimulationMarker.Auto())
            {
                var remaining = math.min(elapsed, 4d / 60);
                var data = this.Compiled.Data;
                for (var step = 0; step < 4; step++)
                {
                    var delta = math.min(remaining, 1d / 60);
                    UIParticleSimulation.Step(ref data, ref this.Particles, ref this.States, ref this.streams, delta, this.seed,
                        this.emitting, ref spawnTransform);
                    remaining -= delta;
                    if (remaining <= 0)
                    {
                        break;
                    }
                }
            }

            this.UpdatePlaying();
            return true;
        }

        internal void PrepareMesh(float4 tint)
        {
            this.ThrowIfDisposed();
            using (AppearanceMarker.Auto())
            {
                var data = this.Compiled.Data;
                UIParticleMesh.Fill(ref data, ref this.Particles, ref this.States, ref this.Quads, ref this.QuadCounts, ref tint);
            }
        }

        internal void Draw(MeshGenerationContext context, ref float4x4 simulationToLocal, bool panelSpace)
        {
            var offset = 0;
            for (var e = 0; e < this.QuadCounts.Length;)
            {
                var texture = this.Compiled.Textures[e];
                var count = this.QuadCounts[e++];
                while (e < this.QuadCounts.Length && this.Compiled.Textures[e] == texture)
                {
                    count = checked(count + this.QuadCounts[e++]);
                }

                var quads = this.Quads.Slice(offset, count);
                ParticleQuadMesh.Draw(context, quads, texture, simulationToLocal, panelSpace);
                offset += count;
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.Particles.Dispose();
            this.States.Dispose();
            this.Quads.Dispose();
            this.QuadCounts.Dispose();
            this.streams.Dispose();
            this.Compiled.Release();
            this.disposed = true;
            this.IsPlaying = false;
            this.IsPaused = false;
        }

        private void UpdatePlaying()
        {
            var active = false;
            for (var e = 0; e < this.States.Length; e++)
            {
                var state = this.States[e];
                var settings = this.Compiled.Data.Emitters[e];
                active |= state.Count != 0 || (this.emitting && (settings.Looping || state.Time < settings.Delay + settings.Duration));
            }

            this.IsPlaying = active;
            if (!active)
            {
                this.IsPaused = false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(UIParticleRuntime));
            }
        }
    }
}
