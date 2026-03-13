// <copyright file="MemoryToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Profiling;

    public class MemoryToolbarViewModel : ObservableObject
    {
        private float timeToTriggerUpdatesPassed;

        private int totalAllocatedMemoryMB;

        private int totalReservedMemoryMB;

        private int monoUsedSizeMB;

        private int allocatedMemoryForGraphicsMB;

        [CreateProperty]
        public int TotalAllocatedMemoryMB
        {
            get => this.totalAllocatedMemoryMB;
            set => this.SetProperty(ref this.totalAllocatedMemoryMB, value);
        }

        [CreateProperty]
        public int TotalReservedMemoryMB
        {
            get => this.totalReservedMemoryMB;
            set => this.SetProperty(ref this.totalReservedMemoryMB, value);
        }

        [CreateProperty]
        public int MonoUsedSizeMB
        {
            get => this.monoUsedSizeMB;
            set => this.SetProperty(ref this.monoUsedSizeMB, value);
        }

        [CreateProperty]
        public int AllocatedMemoryForGraphicsMB
        {
            get => this.allocatedMemoryForGraphicsMB;
            set => this.SetProperty(ref this.allocatedMemoryForGraphicsMB, value);
        }

        public void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            this.timeToTriggerUpdatesPassed += unscaledDeltaTime;

            if (this.timeToTriggerUpdatesPassed < ToolbarView.UpdateRateSeconds)
            {
                return;
            }

            const float megaByte = 1024 * 1024;

            this.TotalAllocatedMemoryMB = Mathf.CeilToInt(Profiler.GetTotalAllocatedMemoryLong() / megaByte);
            this.TotalReservedMemoryMB = Mathf.CeilToInt(Profiler.GetTotalReservedMemoryLong() / megaByte);
            this.MonoUsedSizeMB = Mathf.CeilToInt(Profiler.GetMonoUsedSizeLong() / megaByte);
            this.AllocatedMemoryForGraphicsMB = Mathf.CeilToInt(Profiler.GetAllocatedMemoryForGraphicsDriver() / megaByte);
        }
    }
}
