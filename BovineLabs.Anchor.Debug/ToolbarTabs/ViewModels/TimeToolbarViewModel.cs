// <copyright file="TimeToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels
{
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;

    public class TimeToolbarViewModel : BLObservableObject
    {
        private float timescale;

        [CreateProperty]
        public float TimeScale
        {
            get => this.timescale;
            set
            {
                value = math.clamp(value, 0, 100);
                if (this.SetProperty(ref this.timescale, value))
                {
                    Time.timeScale = this.TimeScale;
                }
            }
        }

        [CreateProperty]
        public long UnscaledSeconds => (long)Time.unscaledTimeAsDouble;

        [CreateProperty]
        public long Seconds => (long)Time.timeAsDouble;

        public void Update()
        {
            this.TimeScale = Time.timeScale;
        }
    }
}
