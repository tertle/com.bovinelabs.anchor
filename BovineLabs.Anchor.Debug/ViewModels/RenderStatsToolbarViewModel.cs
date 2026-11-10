namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using System.Text;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Core.Utility;
    using Unity.Profiling;
    using Unity.Properties;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Render Stats")]
    public class RenderStatsToolbarViewModel : ObservableObject, IToolbarElement, ILoadable
    {
        [NoAutoStaticsCleanup]
        private static readonly string[] CounterGroups =
        {
            "Standard",
            "Standard Indirect",
            "Standard Instanced",
            "SRP Batcher",
            "BRG",
            "BRG Indirect",
            "Null Geometry",
            "Null Geometry Indirect",
        };

        private readonly long[] _drawCallCounts = new long[CounterGroups.Length];
        private readonly long[] _instanceCounts = new long[CounterGroups.Length];

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

        [CreateProperty(ReadOnly = true)]
        public string DrawCallsBreakdown => FormatBreakdown(_drawCallCounts);

        [CreateProperty(ReadOnly = true)]
        public string InstancesBreakdown => FormatBreakdown(_instanceCounts);

        public VisualElement CreateElement()
        {
            return new RenderStatsToolbarView(this);
        }

        public void Load()
        {
            _trianglesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Triangles Count");
            _verticesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Vertices Count");
            _setPassCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "SetPass Calls Count");
            _drawCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, Array.ConvertAll(CounterGroups, static group => $"{group} Draw Calls Count"));
            _instancesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, Array.ConvertAll(CounterGroups, static group => $"{group} Instances Count"));
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
            DrawCalls = SampleCounters(_drawCallsRecorder, _drawCallCounts);
            Instances = SampleCounters(_instancesRecorder, _instanceCounts);
            OnPropertyChanged(nameof(DrawCallsBreakdown));
            OnPropertyChanged(nameof(InstancesBreakdown));
        }

        private static long SampleCounters(ProfilerRecorderGroup recorder, long[] counts)
        {
            var total = 0L;
            for (var i = 0; i < counts.Length; i++)
            {
                var count = recorder.GetLastValue(i);
                counts[i] = count;
                total += count;
            }

            return total;
        }

        private static string FormatBreakdown(long[] counts)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < counts.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(CounterGroups[i]).Append(": ").Append(counts[i].ToString("N0"));
            }

            return builder.ToString();
        }
    }
}
