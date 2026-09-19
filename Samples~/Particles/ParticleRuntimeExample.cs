namespace BovineLabs.Anchor.Particles.Sample
{
    using UnityEngine;

    public static class ParticleRuntimeExample
    {
        public static UIParticleEffect CreateEffect(string preset, Texture2D texture)
        {
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            var emitter = effect.Emitters[0];
            emitter.Texture = texture;
            switch (preset)
            {
                case "Sparkle":
                    emitter.Speed = new Vector2(15, 65);
                    emitter.Size = new Vector2(6, 14);
                    emitter.StartColorMin = new Color(1, 0.65f, 0.2f);
                    emitter.StartColorMax = new Color(1, 1, 0.8f);
                    break;
                case "Confetti":
                    emitter.MaxParticles = 256;
                    emitter.Bursts[0] = new UIParticleBurst
                    {
                        Count = 100,
                    };
                    emitter.Speed = new Vector2(90, 180);
                    emitter.Direction = -90;
                    emitter.Spread = 95;
                    emitter.Lifetime = new Vector2(1, 2);
                    emitter.Size = new Vector2(4, 9);
                    emitter.Acceleration = new Vector2(0, 160);
                    emitter.AngularVelocity = new Vector2(-240, 240);
                    emitter.StartColorMin = new Color(0.2f, 0.4f, 0.7f);
                    emitter.StartColorMax = new Color(1, 0.8f, 1);
                    break;
                case "Dust":
                    emitter.Looping = true;
                    emitter.Bursts.Clear();
                    emitter.Rate = 12;
                    emitter.Shape = UIParticleShape.Rectangle;
                    emitter.Dimensions = new Vector2(240, 160);
                    emitter.Lifetime = new Vector2(2, 4);
                    emitter.Speed = new Vector2(3, 8);
                    emitter.Direction = -90;
                    emitter.Spread = 25;
                    emitter.Size = new Vector2(3, 6);
                    emitter.StartColorMin = new Color(0.5f, 0.7f, 1, 0.25f);
                    emitter.StartColorMax = new Color(0.8f, 0.9f, 1, 0.5f);
                    break;
                case "Layered":
                    emitter.Bursts[0] = new UIParticleBurst
                    {
                        Count = 1,
                    };
                    emitter.Speed = Vector2.zero;
                    emitter.Size = new Vector2(70, 70);
                    emitter.Lifetime = new Vector2(0.25f, 0.25f);
                    emitter.StartColorMin = new Color(1, 0.65f, 0.3f, 0.5f);
                    emitter.StartColorMax = emitter.StartColorMin;
                    effect.Emitters.Add(new UIParticleEmitterSettings
                    {
                        Texture = texture,
                        Speed = new Vector2(70, 150),
                        Size = new Vector2(4, 9),
                        Acceleration = new Vector2(0, 45),
                        Drag = 0.5f,
                        Bursts = new()
                        {
                            new UIParticleBurst
                            {
                                Count = 48,
                            },
                        },
                        StartColorMin = new Color(1, 0.4f, 0.1f),
                        StartColorMax = new Color(1, 0.9f, 0.5f),
                    });
                    effect.Emitters.Add(new UIParticleEmitterSettings
                    {
                        Texture = texture,
                        Shape = UIParticleShape.Circle,
                        Dimensions = new Vector2(100, 100),
                        Speed = Vector2.zero,
                        StartDelay = 0.15f,
                        Size = new Vector2(2, 5),
                        Bursts = new()
                        {
                            new UIParticleBurst
                            {
                                Count = 32,
                            },
                        },
                    });
                    break;
            }

            return effect;
        }
    }
}
