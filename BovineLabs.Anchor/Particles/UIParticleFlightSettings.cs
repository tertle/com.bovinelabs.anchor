namespace BovineLabs.Anchor.Particles
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class UIParticleFlightSettings
    {
        public bool Enabled;
        public Vector2 ScatterDuration = new(0.1f, 0.2f);
        public Vector2 FlightDuration = new(0.35f, 0.6f);
        public float PathBend = 40;
        public float ArrivalRadius = 2;
        public UIParticleTargetMode TargetMode;
    }
}
