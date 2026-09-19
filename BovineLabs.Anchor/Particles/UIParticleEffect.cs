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
