namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorParticles : VisualElement
    {
        public const string UssClassName = "bl-anchor-particles";
        private UIParticleEffect effect;
        private UIParticleRuntime runtime;
        private UIParticleCoordinator coordinator;
        private UIParticleSpace simulationSpace;
        private UIParticleTimeMode timeMode;
        private UIParticleHiddenBehaviour hiddenBehaviour;
        private Color tint = Color.white;
        private bool playOnAttach = true;
        private bool effectsEnabled = true;
        private float playbackSpeed = 1;
        private uint seed = 1;
        private bool requested;
        private bool pending;
        private bool explicitlyPaused;
        private bool hiddenPaused;
        private bool completionPending;
        private VisualElement source;
        private Vector2 sourcePoint;
        private readonly List<VisualElement> ancestry = new();

        public AnchorParticles()
        {
            this.AddToClassList(UssClassName);
            this.pickingMode = PickingMode.Ignore;
            this.focusable = false;
            this.generateVisualContent += this.Draw;
            this.RegisterCallback<AttachToPanelEvent>(this.Attach);
            this.RegisterCallback<DetachFromPanelEvent>(this.Detach);
            this.RegisterCallback<GeometryChangedEvent>(this.GeometryChanged);
        }

        public event Action Completed;

        [CreateProperty, UxmlAttribute]
        public UIParticleEffect Effect
        {
            get => this.effect;
            set
            {
                if (this.effect == value)
                {
                    return;
                }

                var restart = this.requested;
                this.Release();
                this.effect = value;
                if (restart)
                {
                    this.Play();
                }

                this.NotifyPropertyChanged(nameof(this.Effect));
            }
        }

        [CreateProperty, UxmlAttribute]
        public bool PlayOnAttach
        {
            get => this.playOnAttach;
            set
            {
                if (this.playOnAttach == value)
                {
                    return;
                }

                this.playOnAttach = value;
                this.NotifyPropertyChanged(nameof(this.PlayOnAttach));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleSpace SimulationSpace
        {
            get => this.simulationSpace;
            set
            {
                if (this.simulationSpace == value)
                {
                    return;
                }

                this.simulationSpace = value;
                this.RestartRequested();
                this.NotifyPropertyChanged(nameof(this.SimulationSpace));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleTimeMode TimeMode
        {
            get => this.timeMode;
            set
            {
                if (this.timeMode == value)
                {
                    return;
                }

                this.timeMode = value;
                this.RestartRequested();
                this.NotifyPropertyChanged(nameof(this.TimeMode));
            }
        }

        [CreateProperty, UxmlAttribute]
        public float PlaybackSpeed
        {
            get => this.playbackSpeed;
            set
            {
                if (!float.IsFinite(value) || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (this.playbackSpeed == value)
                {
                    return;
                }

                this.playbackSpeed = value;
                this.coordinator?.Wake();
                this.NotifyPropertyChanged(nameof(this.PlaybackSpeed));
            }
        }

        [CreateProperty, UxmlAttribute]
        public uint Seed
        {
            get => this.seed;
            set
            {
                if (this.seed == value)
                {
                    return;
                }

                this.seed = value;
                this.RestartRequested();
                this.NotifyPropertyChanged(nameof(this.Seed));
            }
        }

        [CreateProperty, UxmlAttribute]
        public Color Tint
        {
            get => this.tint;
            set
            {
                if (this.tint == value)
                {
                    return;
                }

                this.tint = value;
                if (this.LiveCount != 0)
                {
                    this.MarkDirtyRepaint();
                }

                this.NotifyPropertyChanged(nameof(this.Tint));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleHiddenBehaviour HiddenBehaviour
        {
            get => this.hiddenBehaviour;
            set
            {
                if (this.hiddenBehaviour == value)
                {
                    return;
                }

                this.hiddenBehaviour = value;
                this.coordinator?.Wake();
                this.NotifyPropertyChanged(nameof(this.HiddenBehaviour));
            }
        }

        [CreateProperty, UxmlAttribute]
        public bool EffectsEnabled
        {
            get => this.effectsEnabled;
            set
            {
                if (this.effectsEnabled == value)
                {
                    return;
                }

                this.effectsEnabled = value;
                if (!value)
                {
                    this.Clear();
                }

                this.NotifyPropertyChanged(nameof(this.EffectsEnabled));
            }
        }

        [CreateProperty] public bool IsPlaying => this.pending || (this.runtime?.IsPlaying ?? false);
        [CreateProperty] public bool IsPaused => this.explicitlyPaused || this.hiddenPaused;
        public int LiveCount => this.runtime?.LiveCount ?? 0;
        public ulong DroppedCount => this.runtime?.DroppedCount ?? 0;
        internal bool NeedsUpdate => this.completionPending || (this.IsPlaying &&
            ((!this.explicitlyPaused && this.playbackSpeed > 0) || this.hiddenBehaviour == UIParticleHiddenBehaviour.StopAndClear));
        internal bool ManualClock { get; set; }

        public void Play(uint seed)
        {
            var changed = this.seed != seed;
            this.seed = seed;
            if (changed)
            {
                this.NotifyPropertyChanged(nameof(this.Seed));
            }
            this.Play();
        }

        public void Play()
        {
            this.Clear();
            if (!this.effectsEnabled)
            {
                return;
            }

            this.requested = true;
            this.pending = this.effect != null;
            if (this.panel != null)
            {
                this.coordinator = UIParticleCoordinator.Register(this.panel, this);
            }

            this.coordinator?.Wake();
            this.Tick(0);
        }

        // Source is a coordinate input for future births, not a target or a reparenting instruction.
        public void SetSourcePoint(VisualElement source, Vector2 localPoint)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (this.panel == null || source.panel != this.panel)
            {
                throw new ArgumentException("Particle source and renderer must belong to the same panel.", nameof(source));
            }

            if (!float.IsFinite(localPoint.x) || !float.IsFinite(localPoint.y))
            {
                throw new ArgumentOutOfRangeException(nameof(localPoint));
            }

            this.source = source;
            this.sourcePoint = localPoint;
        }

        public void ResetSourcePoint()
        {
            this.source = null;
            this.sourcePoint = default;
        }

        public void StopEmitting()
        {
            if (this.pending)
            {
                this.Clear();
                return;
            }

            this.pending = false;
            this.runtime?.StopEmitting();
            this.CheckCompletion();
            this.coordinator?.Wake();
        }

        public void Pause()
        {
            this.explicitlyPaused = this.IsPlaying;
            this.runtime?.Pause();
        }

        public void Resume()
        {
            this.explicitlyPaused = false;
            this.runtime?.Resume();
            this.coordinator?.Wake();
        }

        public new void Clear()
        {
            var hadParticles = this.LiveCount != 0;
            this.requested = false;
            this.pending = false;
            this.completionPending = false;
            this.explicitlyPaused = false;
            this.hiddenPaused = false;
            this.runtime?.Clear();
            if (hadParticles)
            {
                this.MarkDirtyRepaint();
            }
        }

        // Editor previews call this explicitly. Player panels are advanced by their coordinator instead.
        public void Advance(double elapsed)
        {
            this.Tick(elapsed);
            this.DispatchCompletion();
        }

        internal void Tick(double elapsed)
        {
            if (!double.IsFinite(elapsed) || elapsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            }

            if (!this.NeedsUpdate || this.panel == null)
            {
                return;
            }

            var hidden = this.IsHidden();
            if (hidden && this.hiddenBehaviour == UIParticleHiddenBehaviour.StopAndClear)
            {
                this.Clear();
                return;
            }

            if (this.explicitlyPaused || this.playbackSpeed == 0)
            {
                return;
            }

            var wasHiddenPaused = this.hiddenPaused;
            this.hiddenPaused = hidden && this.hiddenBehaviour == UIParticleHiddenBehaviour.Pause;
            if (this.hiddenPaused || !this.TryGetTransforms(out var spawn, out _))
            {
                return;
            }

            if (wasHiddenPaused)
            {
                elapsed = 0;
            }

            if (this.pending)
            {
                if (this.runtime == null || this.runtime.Revision != this.effect.Revision)
                {
                    this.runtime?.Dispose();
                    this.runtime = null;
                    this.runtime = new UIParticleRuntime(this.effect);
                }

                this.runtime.Play(this.seed);
                this.pending = false;
                elapsed = 0;
            }

            if (this.runtime == null)
            {
                return;
            }

            var hadParticles = this.runtime.LiveCount != 0;
            if (this.runtime.Advance(elapsed * this.playbackSpeed, ref spawn) && !hidden && (hadParticles || this.runtime.LiveCount != 0))
            {
                this.MarkDirtyRepaint();
            }

            this.CheckCompletion();
        }

        internal void DispatchCompletion()
        {
            if (!this.completionPending)
            {
                return;
            }

            this.completionPending = false;
            this.Completed?.Invoke();
        }

        internal void Release()
        {
            this.Clear();
            this.runtime?.Dispose();
            this.runtime = null;
            this.ResetSourcePoint();
            this.ancestry.Clear();
        }

        private void CheckCompletion()
        {
            if (this.requested && !this.pending && this.runtime != null && !this.runtime.IsPlaying)
            {
                this.requested = false;
                this.completionPending = true;
            }
        }

        private void RestartRequested()
        {
            if (this.requested)
            {
                this.Play();
            }
        }

        private bool IsHidden()
        {
            var rebuild = this.ancestry.Count == 0;
            for (var i = 0; i < this.ancestry.Count && !rebuild; i++)
            {
                var parent = i + 1 < this.ancestry.Count ? this.ancestry[i + 1] : null;
                rebuild = this.ancestry[i].parent != parent;
            }

            if (rebuild)
            {
                this.ancestry.Clear();
                for (var element = (VisualElement)this; element != null; element = element.parent)
                {
                    this.ancestry.Add(element);
                }
            }

            foreach (var element in this.ancestry)
            {
                if (element.resolvedStyle.display == DisplayStyle.None || element.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetTransforms(out float4x4 spawn, out float4x4 render)
        {
            spawn = float4x4.identity;
            render = float4x4.identity;
            if (this.pending && (!float.IsFinite(this.layout.width) || !float.IsFinite(this.layout.height) ||
                this.layout.width <= 0 || this.layout.height <= 0))
            {
                return false;
            }

            var world = this.worldTransform;
            var determinant = (world.m00 * world.m11) - (world.m01 * world.m10);
            if (!float.IsFinite(determinant) || Mathf.Abs(determinant) < 0.000001f)
            {
                return false;
            }

            var sourceWorld = world;
            if (this.source != null)
            {
                // A source removed independently is a documented stale coordinate request; suspend until replaced.
                if (this.source.panel != this.panel)
                {
                    return false;
                }

                sourceWorld = this.source.worldTransform * Matrix4x4.Translate(this.sourcePoint);
                determinant = (sourceWorld.m00 * sourceWorld.m11) - (sourceWorld.m01 * sourceWorld.m10);
                if (!float.IsFinite(determinant) || Mathf.Abs(determinant) < 0.000001f)
                {
                    return false;
                }
            }

            if (this.simulationSpace == UIParticleSpace.Panel)
            {
                spawn = sourceWorld;
                render = world.inverse;
            }
            else if (this.source != null)
            {
                spawn = world.inverse * sourceWorld;
            }

            return true;
        }

        private void Attach(AttachToPanelEvent evt)
        {
            this.coordinator = UIParticleCoordinator.Register(evt.destinationPanel, this);
            if (this.effectsEnabled && this.playOnAttach && Application.isPlaying)
            {
                this.Play();
            }
            else if (this.requested)
            {
                this.coordinator.Wake();
            }
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            this.ReleaseVisualGeneration();
        }

        internal void ReleaseVisualGeneration()
        {
            this.coordinator?.Unregister(this);
            this.coordinator = null;
            this.Release();
            this.Completed = null;
        }

        private void GeometryChanged(GeometryChangedEvent evt)
        {
            if (this.pending)
            {
                this.Advance(0);
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            if (this.runtime == null || this.runtime.LiveCount == 0 || this.IsHidden() || !this.TryGetTransforms(out _, out var render))
            {
                return;
            }

            this.runtime.PrepareMesh(new float4(this.tint.r, this.tint.g, this.tint.b, this.tint.a));
            this.runtime.Draw(context, ref render, this.simulationSpace == UIParticleSpace.Panel);
        }
    }
}
