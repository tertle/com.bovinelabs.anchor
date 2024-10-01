// <copyright file="MemoryToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.MVVM;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine.Profiling;

    public class MemoryToolbarViewModel : ObservableObject
    {
        private int totalAllocatedMemoryMB;
        private int totalReservedMemoryMB;
        private int monoUsedSizeMB;
        private int allocatedMemoryForGraphicsMB;
        private float timeToTriggerUpdatesPassed;

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
            var unscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;
            this.timeToTriggerUpdatesPassed += unscaledDeltaTime;

            if (this.timeToTriggerUpdatesPassed < ToolbarView.DefaultUpdateRate)
            {
                return;
            }

            const float megaByte = 1024 * 1024;

            this.TotalAllocatedMemoryMB = (int)math.ceil(Profiler.GetTotalAllocatedMemoryLong() / megaByte);
            this.TotalReservedMemoryMB = (int)math.ceil(Profiler.GetTotalReservedMemoryLong() / megaByte);
            this.MonoUsedSizeMB = (int)math.ceil(Profiler.GetMonoUsedSizeLong() / megaByte);
            this.AllocatedMemoryForGraphicsMB = (int)math.ceil(Profiler.GetAllocatedMemoryForGraphicsDriver() / megaByte);
        }
    }
}
