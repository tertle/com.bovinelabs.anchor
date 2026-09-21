namespace BovineLabs.Anchor.Particles
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "BovineLabs/Anchor/Particle Effect")]
    public sealed class UIParticleEffect : ScriptableObject
    {
        [SerializeField] private List<UIParticleEmitterSettings> _emitters = new() { new UIParticleEmitterSettings() };
        private UIParticleCompiledEffect _compiled;

        public List<UIParticleEmitterSettings> Emitters => _emitters;
        public uint Revision { get; private set; }

        private void OnValidate() => Invalidate();

        // Explicit authoring boundary: existing instances retain their immutable revision until disposal.
        public void Invalidate()
        {
            Revision++;
            _compiled = null;
        }

        internal int GetCapacity()
        {
            var capacity = 0;
            foreach (var emitter in _emitters)
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
            if (_compiled == null || _compiled.IsDisposed)
            {
                _compiled = new UIParticleCompiledEffect(this);
            }

            _compiled.Retain();
            return _compiled;
        }
    }
}
