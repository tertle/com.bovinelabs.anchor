// <copyright file="TimeToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using Unity.AppUI.MVVM;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;

    [ObservableObject]
    public partial class TimeToolbarViewModel
    {
        private static readonly float[] TimescaleValues = { 0.1f, 0.25f, 0.5f, 0.75f, 1f, 2f, 4f, 8f, 16f };
        private float timescale;

        [CreateProperty]
        public float TimeScale
        {
            get => this.timescale;
            set
            {
                value = Mathf.Clamp(value, 0, 100);
                if (this.SetProperty(ref this.timescale, value))
                {
                    Time.timeScale = this.TimeScale;
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public long UnscaledSeconds => (long)Time.unscaledTimeAsDouble;

        [CreateProperty(ReadOnly = true)]
        public long Seconds => (long)Time.timeAsDouble;

        public void Update()
        {
            this.TimeScale = Time.timeScale;
        }

        public static float TimescaleToUI(float value)
        {
            return value switch
            {
                <= 2 => value,
                _ => (value + 14) / 8,
            };
        }

        public static float UIToTimeScale(float value)
        {
            return value switch
            {
                <= 0 => 0.1f,
                <= 2 => value,
                _ => (8 * value) - 14,
            };
        }
    }
}
