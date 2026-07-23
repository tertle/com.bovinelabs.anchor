// <copyright file="RenderingToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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

        private ProfilerRecorderGroup trianglesRecorder;
        private ProfilerRecorderGroup verticesRecorder;
        private ProfilerRecorderGroup setPassCallsRecorder;
        private ProfilerRecorderGroup drawCallsRecorder;
        private ProfilerRecorderGroup instancesRecorder;

        private float timeToTriggerUpdatesPassed;
        private long triangles;
        private long vertices;
        private long drawCalls;
        private long setPassCalls;
        private long instances;

        [CreateProperty]
        public long Triangles
        {
            get => this.triangles;
            set => this.SetProperty(ref this.triangles, value);
        }

        [CreateProperty]
        public long Vertices
        {
            get => this.vertices;
            set => this.SetProperty(ref this.vertices, value);
        }

        [CreateProperty]
        public long DrawCalls
        {
            get => this.drawCalls;
            set => this.SetProperty(ref this.drawCalls, value);
        }

        [CreateProperty]
        public long SetPassCalls
        {
            get => this.setPassCalls;
            set => this.SetProperty(ref this.setPassCalls, value);
        }

        [CreateProperty]
        public long Instances
        {
            get => this.instances;
            set => this.SetProperty(ref this.instances, value);
        }

        /// <inheritdoc />
        public VisualElement CreateElement()
        {
            return new RenderingToolbarView(this);
        }

        /// <inheritdoc />
        public void Load()
        {
            this.trianglesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Triangles Count");
            this.verticesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "Vertices Count");
            this.setPassCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, "SetPass Calls Count");
            this.drawCallsRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, DrawCallCounterNames);
            this.instancesRecorder = new ProfilerRecorderGroup(ProfilerCategory.Render, InstanceCounterNames);
        }

        /// <inheritdoc />
        public void Unload()
        {
            this.trianglesRecorder.Dispose();
            this.verticesRecorder.Dispose();
            this.setPassCallsRecorder.Dispose();
            this.drawCallsRecorder.Dispose();
            this.instancesRecorder.Dispose();
        }

        public void Update()
        {
            this.timeToTriggerUpdatesPassed += Time.unscaledDeltaTime;

            if (this.timeToTriggerUpdatesPassed < Toolbar.UpdateRateSeconds)
            {
                return;
            }

            this.timeToTriggerUpdatesPassed = 0;
            this.Triangles = this.trianglesRecorder.LastValue;
            this.Vertices = this.verticesRecorder.LastValue;
            this.SetPassCalls = this.setPassCallsRecorder.LastValue;
            this.DrawCalls = this.drawCallsRecorder.LastValue;
            this.Instances = this.instancesRecorder.LastValue;
        }
    }
}
