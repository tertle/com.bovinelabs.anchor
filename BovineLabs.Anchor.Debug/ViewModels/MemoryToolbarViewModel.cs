namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Profiling;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Memory")]
    public class MemoryToolbarViewModel : ObservableObject, IToolbarElement
    {
        private float _timeToTriggerUpdatesPassed;

        private int _totalAllocatedMemoryMB;

        private int _totalReservedMemoryMB;

        private int _textureMemoryMB;

        private int _allocatedMemoryForGraphicsMB;

        [CreateProperty]
        public int TotalAllocatedMemoryMB
        {
            get => _totalAllocatedMemoryMB;
            set => SetProperty(ref _totalAllocatedMemoryMB, value);
        }

        [CreateProperty]
        public int TotalReservedMemoryMB
        {
            get => _totalReservedMemoryMB;
            set => SetProperty(ref _totalReservedMemoryMB, value);
        }

        [CreateProperty]
        public int TextureMemoryMB
        {
            get => _textureMemoryMB;
            set => SetProperty(ref _textureMemoryMB, value);
        }

        [CreateProperty]
        public int AllocatedMemoryForGraphicsMB
        {
            get => _allocatedMemoryForGraphicsMB;
            set => SetProperty(ref _allocatedMemoryForGraphicsMB, value);
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
