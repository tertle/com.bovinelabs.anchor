// <copyright file="TimeToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Time")]
    public class TimeToolbarViewModel : ObservableObject, IToolbarElement
    {
        private float timescale;
        private long unscaledSeconds;
        private long seconds;

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
        public long UnscaledSeconds
        {
            get => this.unscaledSeconds;
            set => this.SetProperty(ref this.unscaledSeconds, value);
        }

        [CreateProperty(ReadOnly = true)]
        public long Seconds
        {
            get => this.seconds;
            set => this.SetProperty(ref this.seconds, value);
        }

        /// <inheritdoc />
        public VisualElement CreateElement()
        {
            return new TimeToolbarView(this);
        }

        public void Update()
        {
            this.TimeScale = Time.timeScale;
            this.UnscaledSeconds = (long)Time.unscaledTimeAsDouble;
            this.Seconds = (long)Time.timeAsDouble;
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
