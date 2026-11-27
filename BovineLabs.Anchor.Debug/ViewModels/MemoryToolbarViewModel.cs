namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using UnityEngine;
    using UnityEngine.Profiling;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Memory")]
    public partial class MemoryToolbarViewModel : ObservableObject, IToolbarElement
    {
        private float _timeToTriggerUpdatesPassed;

        [ObservableProperty]
        public partial int TotalAllocatedMemoryMB
        {
            get;
            set;
        }

        [ObservableProperty]
        public partial int TotalReservedMemoryMB
        {
            get;
            set;
        }

        [ObservableProperty]
        public partial int TextureMemoryMB
        {
            get;
            set;
        }

        [ObservableProperty]
        public partial int AllocatedMemoryForGraphicsMB
        {
            get;
            set;
        }

        public VisualElement CreateElement()
        {
            return new MemoryToolbarView(this);
        }

        public void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            _timeToTriggerUpdatesPassed += unscaledDeltaTime;

            if (_timeToTriggerUpdatesPassed < Toolbar.UpdateRateSeconds)
            {
                return;
            }

            _timeToTriggerUpdatesPassed = 0;

            const float megaByte = 1024 * 1024;

            TotalAllocatedMemoryMB = Mathf.CeilToInt(Profiler.GetTotalAllocatedMemoryLong() / megaByte);
            TotalReservedMemoryMB = Mathf.CeilToInt(Profiler.GetTotalReservedMemoryLong() / megaByte);
            AllocatedMemoryForGraphicsMB = Mathf.CeilToInt(Profiler.GetAllocatedMemoryForGraphicsDriver() / megaByte);
            TextureMemoryMB = Mathf.CeilToInt(Texture.currentTextureMemory / megaByte);
        }
    }
}
