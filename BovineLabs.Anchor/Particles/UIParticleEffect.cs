namespace BovineLabs.Anchor.Particles
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "BovineLabs/Anchor/Particle Effect")]
    public sealed class UIParticleEffect : ScriptableObject
    {
        [SerializeField] private List<UIParticleEmitterSettings> emitters = new() { new UIParticleEmitterSettings() };
        private UIParticleCompiledEffect compiled;

        public List<UIParticleEmitterSettings> Emitters => this.emitters;
        public uint Revision { get; private set; }

        private void OnValidate() => this.Invalidate();

        // Explicit authoring boundary: existing instances retain their immutable revision until disposal.
        public void Invalidate()
        {
            this.Revision++;
            this.compiled = null;
        }

        internal int GetCapacity()
        {
            var capacity = 0;
            foreach (var emitter in this.emitters)
            {
                if (emitter == null || emitter.MaxParticles <= 0)
                {
                    throw new System.ArgumentException("Emitter capacity must be positive.");
                }

                capacity = checked(capacity + emitter.MaxParticles);
            }

            return capacity;
        }

        internal UIParticleCompiledEffect Acquire()
        {
            if (this.compiled == null || this.compiled.IsDisposed)
            {
                this.compiled = new UIParticleCompiledEffect(this);
            }

            this.compiled.Retain();
            return this.compiled;
        }
    }
}
