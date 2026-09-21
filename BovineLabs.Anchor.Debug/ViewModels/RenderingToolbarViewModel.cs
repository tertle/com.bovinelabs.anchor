namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Core.Utility;
    using Unity.Profiling;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Rendering")]
    public class RenderingToolbarViewModel : ObservableObject, IToolbarElement, ILoadable
    {
        private static readonly string[] DrawCallCounterNames =
        {
            "Standard Draw Calls Count",
            "Standard Indirect Draw Calls Count",
            "Standard Instanced Draw Calls Count",
            "SRP Batcher Draw Calls Count",
            "BRG Draw Calls Count",
            "BRG Indirect Draw Calls Count",
            "Null Geometry Draw Calls Count",
            "Null Geometry Indirect Draw Calls Count",
        };

        private static readonly string[] InstanceCounterNames =
        {
            "Standard Instances Count",
            "Standard Indirect Instances Count",
            "Standard Instanced Instances Count",
            "SRP Batcher Instances Count",
            "BRG Instances Count",
            "BRG Indirect Instances Count",
            "Null Geometry Instances Count",
            "Null Geometry Indirect Instances Count",
        };

        private ProfilerRecorderGroup _trianglesRecorder;
        private ProfilerRecorderGroup _verticesRecorder;
        private ProfilerRecorderGroup _setPassCallsRecorder;
        private ProfilerRecorderGroup _drawCallsRecorder;
        private ProfilerRecorderGroup _instancesRecorder;

        private float _timeToTriggerUpdatesPassed;
        private long _triangles;
        private long _vertices;
        private long _drawCalls;
        private long _setPassCalls;
        private long _instances;

        [CreateProperty]
        public long Triangles
        {
            get => _triangles;
            set => SetProperty(ref _triangles, value);
        }

        [CreateProperty]
        public long Vertices
        {
            get => _vertices;
            set => SetProperty(ref _vertices, value);
        }

        [CreateProperty]
        public long DrawCalls
        {
            get => _drawCalls;
            set => SetProperty(ref _drawCalls, value);
        }

        [CreateProperty]
        public long SetPassCalls
        {
            get => _setPassCalls;
            set => SetProperty(ref _setPassCalls, value);
        }

        [CreateProperty]
        public long Instances
        {
            get => _instances;
            set => SetProperty(ref _instances, value);
        }

        public VisualElement CreateElement()
        {
            return new RenderingToolbarView(this);
        }

        public void Load()
        {
            _trianglesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Triangles Count");
            _verticesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Vertices Count");
            _setPassCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "SetPass Calls Count");
            _drawCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, DrawCallCounterNames);
            _instancesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, InstanceCounterNames);
        }

        public void Unload()
        {
            _trianglesRecorder.Dispose();
            _verticesRecorder.Dispose();
            _setPassCallsRecorder.Dispose();
            _drawCallsRecorder.Dispose();
            _instancesRecorder.Dispose();
        }

        public void Update()
        {
            _timeToTriggerUpdatesPassed += Time.unscaledDeltaTime;

            if (_timeToTriggerUpdatesPassed < Toolbar.UpdateRateSeconds)
            {
                return;
            }

            _timeToTriggerUpdatesPassed = 0;
            Triangles = _trianglesRecorder.LastValue;
            Vertices = _verticesRecorder.LastValue;
            SetPassCalls = _setPassCallsRecorder.LastValue;
            DrawCalls = _drawCallsRecorder.LastValue;
            Instances = _instancesRecorder.LastValue;
        }
    }
}
