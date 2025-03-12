﻿// <copyright file="TimeToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using Unity.AppUI.MVVM;
    using Unity.Properties;
    using UnityEngine;

    [ObservableObject]
    public partial class TimeToolbarViewModel
    {
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
    }
}
