// <copyright file="MemoryToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.MVVM;
    using UnityEngine;
    using UnityEngine.Profiling;

    [ObservableObject]
    public partial class MemoryToolbarViewModel
    {
        private float timeToTriggerUpdatesPassed;

        [ObservableProperty]
        private int totalAllocatedMemoryMB;

        [ObservableProperty]
        private int totalReservedMemoryMB;

        [ObservableProperty]
        private int monoUsedSizeMB;

        [ObservableProperty]
        private int allocatedMemoryForGraphicsMB;

        public void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            this.timeToTriggerUpdatesPassed += unscaledDeltaTime;

            if (this.timeToTriggerUpdatesPassed < ToolbarView.DefaultUpdateRate)
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
