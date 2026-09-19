namespace BovineLabs.Anchor.Particles
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public sealed class UIParticleEmitterSettings
    {
        public float Duration = 1;
        public float StartDelay;
        public bool Looping;
        public float Rate;
        public List<UIParticleBurst> Bursts = new()
        {
            new UIParticleBurst
            {
                Time = 0,
                Count = 16,
            },
        };
        public int MaxParticles = 128;
        public UIParticleShape Shape;
        public Vector2 Offset;
        // Full width/height; circle uses X as its diameter, line uses X as its length.
        public Vector2 Dimensions = Vector2.one;
        public float Direction;
        public float Spread = 360;
        public Vector2 Lifetime = new(0.5f, 1);
        public Vector2 Speed = new(20, 60);
        public Vector2 Size = new(4, 8);
        public Vector2 Rotation;
        public Vector2 AngularVelocity;
        public Color StartColorMin = Color.white;
        public Color StartColorMax = Color.white;
        public Vector2 Acceleration;
        public float Drag;
        public AnimationCurve SizeOverLifetime = AnimationCurve.Linear(0, 1, 1, 0);
        public Gradient ColorOverLifetime = new();
        public Texture2D Texture;
        public Rect Uv = new(0, 0, 1, 1);
    }
}
