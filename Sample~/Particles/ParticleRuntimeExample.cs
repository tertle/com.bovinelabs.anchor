namespace BovineLabs.Anchor.Particles.Sample
{
    using UnityEngine;

    public static class ParticleRuntimeExample
    {
        public static UIParticleEffect CreateEffect()
        {
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            effect.hideFlags = HideFlags.DontSave;
            effect.Emitters.Clear();
            effect.Emitters.Add(new UIParticleEmitterSettings
            {
                Offset = new Vector2(200, 160),
                Lifetime = new Vector2(0.2f, 0.2f),
                Speed = Vector2.zero,
                Size = new Vector2(40, 40),
                StartColorMin = new Color(1, 0.7f, 0.2f, 0.4f),
                StartColorMax = new Color(1, 0.7f, 0.2f, 0.4f),
                Bursts = new()
                {
                    new UIParticleBurst
                    {
                        Count = 1,
                    },
                },
            });
            effect.Emitters.Add(new UIParticleEmitterSettings
            {
                Offset = new Vector2(200, 160),
                Lifetime = new Vector2(0.6f, 1.2f),
                Speed = new Vector2(40, 120),
                Size = new Vector2(3, 8),
                Acceleration = new Vector2(0, 40),
                Drag = 0.5f,
                AngularVelocity = new Vector2(-180, 180),
                StartColorMin = new Color(0.6f, 0.3f, 0.1f, 0.5f),
                StartColorMax = new Color(1, 0.8f, 0.4f, 0.8f),
                Bursts = new()
                {
                    new UIParticleBurst
                    {
                        Count = 48,
                    },
                },
            });
            effect.Emitters.Add(new UIParticleEmitterSettings
            {
                Looping = true,
                Rate = 32,
                Shape = UIParticleShape.Rectangle,
                Dimensions = new Vector2(320, 160),
                Offset = new Vector2(200, 160),
                Lifetime = new Vector2(1, 2),
                Speed = new Vector2(5, 15),
                Direction = -90,
                Spread = 30,
                Size = new Vector2(2, 4),
                StartColorMin = new Color(0.3f, 0.6f, 1, 0.2f),
                StartColorMax = new Color(0.5f, 0.8f, 1, 0.5f),
                Bursts = new(),
            });
            return effect;
        }
    }
}
