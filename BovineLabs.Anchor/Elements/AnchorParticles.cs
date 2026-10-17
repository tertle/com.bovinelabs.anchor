namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using Unity.Mathematics;
    using Unity.Properties;
    using Unity.Profiling;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorParticles : VisualElement
    {
        public const string UssClassName = "bl-anchor-particles";
        [NoAutoStaticsCleanup]
        private static readonly ProfilerMarker TargetMarker = new("Anchor.Particles.TargetSample");
        private UIParticleEffect _effect;
        private UIParticleRuntime _runtime;
        private UIParticleCoordinator _coordinator;
        private UIParticleSpace _simulationSpace;
        private UIParticleTimeMode _timeMode;
        private UIParticleHiddenBehaviour _hiddenBehaviour;
        private Color _tint = Color.white;
        private bool _playOnAttach = true;
        private bool _effectsEnabled = true;
        private float _playbackSpeed = 1;
        private float _emissionScale = 1;
        private uint _seed = 1;
        private bool _requested;
        private bool _pending;
        private bool _explicitlyPaused;
        private bool _hiddenPaused;
        private bool _completionPending;
        private int _arrivalsPending;
        private VisualElement _target;
        private string _targetName;
        private Vector2 _targetPoint;
        private VisualElement _source;
        private Vector2 _sourcePoint;
        private Matrix4x4 _renderTransform;
        private readonly List<VisualElement> _ancestry = new();

        public AnchorParticles()
        {
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;
            focusable = false;
            generateVisualContent += Draw;
            RegisterCallback<AttachToPanelEvent>(Attach);
            RegisterCallback<DetachFromPanelEvent>(Detach);
            RegisterCallback<GeometryChangedEvent>(GeometryChanged);
        }

        public event Action Completed;
        // Cosmetic feedback only: at most one aggregate notification after an update, never a gameplay transaction.
        public event Action<int> Arrived;

        [CreateProperty]
        public VisualElement Target
        {
            get => _target;
            set
            {
                if (_target == value)
                {
                    return;
                }

                if (value?.panel != null && panel != null && value.panel != panel)
                {
                    throw new ArgumentException("Particle target and renderer must belong to the same panel.", nameof(value));
                }

                Clear();
                _target?.UnregisterCallback<DetachFromPanelEvent>(TargetDetached);
                _target = value;
                _target?.RegisterCallback<DetachFromPanelEvent>(TargetDetached);
                NotifyPropertyChanged(nameof(Target));
            }
        }

        [CreateProperty, UxmlAttribute]
        public string TargetName
        {
            get => _targetName;
            set
            {
                if (_targetName == value)
                {
                    return;
                }

                Target = null;
                _targetName = value;
                NotifyPropertyChanged(nameof(TargetName));
            }
        }

        [CreateProperty, UxmlAttribute]
        public Vector2 TargetPoint
        {
            get => _targetPoint;
            set
            {
                if (!float.IsFinite(value.x) || !float.IsFinite(value.y))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_targetPoint == value)
                {
                    return;
                }

                _targetPoint = value;
                NotifyPropertyChanged(nameof(TargetPoint));
            }
        }

        // The presenter supplies its current visual generation. A miss stays a miss until explicitly refreshed.
        public void RefreshTarget(VisualElement visualRoot)
        {
            if (visualRoot == null)
            {
                throw new ArgumentNullException(nameof(visualRoot));
            }

            Target = string.IsNullOrEmpty(_targetName) ? null : visualRoot.Q(_targetName);
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleEffect Effect
        {
            get => _effect;
            set
            {
                if (_effect == value)
                {
                    return;
                }

                if (_requested && value != null)
                {
                    Play(value, _seed);
                    return;
                }

                Release();
                _effect = value;

                NotifyPropertyChanged(nameof(Effect));
            }
        }

        [CreateProperty, UxmlAttribute]
        public bool PlayOnAttach
        {
            get => _playOnAttach;
            set
            {
                if (_playOnAttach == value)
                {
                    return;
                }

                _playOnAttach = value;
                NotifyPropertyChanged(nameof(PlayOnAttach));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleSpace SimulationSpace
        {
            get => _simulationSpace;
            set
            {
                if (_simulationSpace == value)
                {
                    return;
                }

                if (value != UIParticleSpace.Panel && _runtime?.HasFlights == true)
                {
                    throw new ArgumentException("A prepared target flight requires Panel simulation space.", nameof(value));
                }

                _simulationSpace = value;
                RestartRequested();
                NotifyPropertyChanged(nameof(SimulationSpace));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleTimeMode TimeMode
        {
            get => _timeMode;
            set
            {
                if (_timeMode == value)
                {
                    return;
                }

                _timeMode = value;
                RestartRequested();
                NotifyPropertyChanged(nameof(TimeMode));
            }
        }

        [CreateProperty, UxmlAttribute]
        public float PlaybackSpeed
        {
            get => _playbackSpeed;
            set
            {
                if (!float.IsFinite(value) || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_playbackSpeed == value)
                {
                    return;
                }

                _playbackSpeed = value;
                _coordinator?.Wake();
                NotifyPropertyChanged(nameof(PlaybackSpeed));
            }
        }

        [CreateProperty, UxmlAttribute]
        public float EmissionScale
        {
            get => _emissionScale;
            set
            {
                if (!float.IsFinite(value) || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_emissionScale == value)
                {
                    return;
                }

                _emissionScale = value;
                if (_runtime != null)
                {
                    _runtime.EmissionScale = value;
                }

                NotifyPropertyChanged(nameof(EmissionScale));
            }
        }

        [CreateProperty, UxmlAttribute]
        public uint Seed
        {
            get => _seed;
            set
            {
                if (_seed == value)
                {
                    return;
                }

                _seed = value;
                RestartRequested();
                NotifyPropertyChanged(nameof(Seed));
            }
        }

        [CreateProperty, UxmlAttribute]
        public Color Tint
        {
            get => _tint;
            set
            {
                if (_tint == value)
                {
                    return;
                }

                _tint = value;
                if (LiveCount != 0)
                {
                    MarkDirtyRepaint();
                }

                NotifyPropertyChanged(nameof(Tint));
            }
        }

        [CreateProperty, UxmlAttribute]
        public UIParticleHiddenBehaviour HiddenBehaviour
        {
            get => _hiddenBehaviour;
            set
            {
                if (_hiddenBehaviour == value)
                {
                    return;
                }

                _hiddenBehaviour = value;
                _coordinator?.Wake();
                NotifyPropertyChanged(nameof(HiddenBehaviour));
            }
        }

        [CreateProperty, UxmlAttribute]
        public bool EffectsEnabled
        {
            get => _effectsEnabled;
            set
            {
                if (_effectsEnabled == value)
                {
                    return;
                }

                _effectsEnabled = value;
                if (!value)
                {
                    Clear();
                }

                NotifyPropertyChanged(nameof(EffectsEnabled));
            }
        }

        [CreateProperty] public bool IsPlaying => _pending || (_runtime?.IsPlaying ?? false);
        [CreateProperty] public bool IsPaused => _explicitlyPaused || _hiddenPaused;
        public int LiveCount => _runtime?.LiveCount ?? 0;
        public ulong DroppedCount => _runtime?.DroppedCount ?? 0;
        public UIParticleCounters Counters => _runtime?.Counters ?? default;
        public int ReservedSlots => _runtime?.Capacity ?? 0;
        public ParticlePlayResult LastPlayResult { get; private set; }
        internal bool NeedsUpdate => (_simulationSpace == UIParticleSpace.Panel && LiveCount != 0) || _completionPending || _arrivalsPending != 0 || (IsPlaying &&
            ((!_explicitlyPaused && _playbackSpeed > 0) || _hiddenBehaviour == UIParticleHiddenBehaviour.StopAndClear));
        internal bool ManualClock { get; set; }

        public ParticlePlayResult Play(uint seed) => Play(_effect, seed);

        public ParticlePlayResult Play() => Play(_effect, _seed);

        public ParticlePlayResult Play(UIParticleEffect effect, uint seed)
        {
            if (!_effectsEnabled)
            {
                return LastPlayResult = ParticlePlayResult.Suppressed;
            }

            if (effect == null)
            {
                Clear();
                return LastPlayResult = ParticlePlayResult.NoEffect;
            }

            if (ValidateFlightSpace(effect) && !HasTarget())
            {
                return LastPlayResult = ParticlePlayResult.TargetUnavailable;
            }

            var hadParticles = LiveCount != 0;
            if (panel != null)
            {
                _coordinator = UIParticleCoordinator.Register(panel, this);
                if (!TryPrepare(effect))
                {
                    return LastPlayResult = ParticlePlayResult.BudgetExceeded;
                }
            }

            var effectChanged = _effect != effect;
            var seedChanged = _seed != seed;
            _effect = effect;
            _seed = seed;
            if (effectChanged)
            {
                ResetSourcePoint();
            }

            _runtime?.Clear();
            _requested = true;
            _pending = true;
            _completionPending = false;
            _arrivalsPending = 0;
            _explicitlyPaused = false;
            _hiddenPaused = false;
            LastPlayResult = ParticlePlayResult.Deferred;
            _coordinator?.Wake();
            Tick(0);
            if (hadParticles && (_pending || LiveCount == 0))
            {
                MarkDirtyRepaint();
            }

            if (effectChanged)
            {
                NotifyPropertyChanged(nameof(Effect));
            }

            if (seedChanged)
            {
                NotifyPropertyChanged(nameof(Seed));
            }

            return LastPlayResult;
        }

        public ParticleEmissionResult Emit(int emitterIndex, int count, VisualElement source, Vector2 localPoint)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!float.IsFinite(localPoint.x) || !float.IsFinite(localPoint.y))
            {
                throw new ArgumentOutOfRangeException(nameof(localPoint));
            }

            if (!_effectsEnabled)
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.Suppressed
                };
            }

            if (_effect == null)
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.NoEffect
                };
            }

            if ((uint)emitterIndex >= (uint)_effect.Emitters.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(emitterIndex));
            }

            var flight = ValidateFlightSpace(_effect);
            if (flight && !HasTarget())
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.TargetUnavailable
                };
            }

            if (panel == null || source.panel == null)
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.SourceUnavailable
                };
            }

            if (source.panel != panel)
            {
                throw new ArgumentException("Particle source and renderer must belong to the same panel.", nameof(source));
            }

            if (!TryGetTransforms(out _, out _, false) || !float.IsFinite(layout.width) || !float.IsFinite(layout.height) || layout.width <= 0 || layout.height <= 0)
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.SourceUnavailable
                };
            }

            var sourceWorld = source.worldTransform * Matrix4x4.Translate(localPoint);
            var determinant = (sourceWorld.m00 * sourceWorld.m11) - (sourceWorld.m01 * sourceWorld.m10);
            if (!float.IsFinite(determinant) || Mathf.Abs(determinant) < 0.000001f)
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.SourceUnavailable
                };
            }

            float4x4 spawn = _simulationSpace == UIParticleSpace.Panel ? sourceWorld : worldTransform.inverse * sourceWorld;
            if (_runtime != null && _runtime.HasFlights && _runtime.Revision != _effect.Revision)
            {
                Clear();
            }

            _coordinator = UIParticleCoordinator.Register(panel, this);
            if (!TryPrepare(_effect))
            {
                return new ParticleEmissionResult {
                    Status = ParticleEmissionStatus.BudgetExceeded
                };
            }

            // A pending Play retains its timeline intent, but the explicit source request is admitted now.
            if (_pending)
            {
                _runtime.Play(_seed);
                _pending = false;
                LastPlayResult = ParticlePlayResult.Started;
            }

            var result = _runtime.Emit(emitterIndex, count, _seed, ref spawn);
            if (result.Emitted != 0)
            {
                _requested = true;
                _completionPending = false;
                MarkDirtyRepaint();
            }

            _coordinator.Wake();
            return result;
        }

        private bool ValidateFlightSpace(UIParticleEffect effect)
        {
            foreach (var emitter in effect.Emitters)
            {
                if (emitter?.Flight?.Enabled == true)
                {
                    if (_simulationSpace != UIParticleSpace.Panel)
                    {
                        throw new ArgumentException("Target flights require Panel simulation space; Local space cannot follow a panel-space destination.");
                    }

                    return true;
                }
            }

            return false;
        }

        private bool HasTarget()
        {
            if (panel == null || _target?.panel == null)
            {
                return false;
            }

            if (_target.panel != panel)
            {
                throw new ArgumentException("Particle target and renderer must belong to the same panel.");
            }

            return true;
        }

        private void TargetDetached(DetachFromPanelEvent evt) => Target = null;

        private bool TryPrepare(UIParticleEffect effect)
        {
            if (_runtime != null && _effect == effect && _runtime.Revision == effect.Revision)
            {
                return true;
            }

            var capacity = effect.GetCapacity();
            if (!_coordinator.TryReserve(capacity))
            {
                return false;
            }

            // Both allocations count during replacement; failure leaves the old run and its reservation untouched.
            UIParticleRuntime replacement = null;
            try
            {
                replacement = new UIParticleRuntime(effect);
            }
            finally
            {
                if (replacement == null)
                {
                    _coordinator.Release(capacity);
                }
            }

            ReleaseStorage();
            _runtime = replacement;
            _runtime.EmissionScale = _emissionScale;
            return true;
        }

        private void ReleaseStorage()
        {
            if (_runtime == null)
            {
                return;
            }

            var capacity = _runtime.Capacity;
            _runtime.Dispose();
            _runtime = null;
            _coordinator.Release(capacity);
        }

        // Source is a coordinate input for future births, not a target or a reparenting instruction.
        public void SetSourcePoint(VisualElement source, Vector2 localPoint)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (panel == null || source.panel != panel)
            {
                throw new ArgumentException("Particle source and renderer must belong to the same panel.", nameof(source));
            }

            if (!float.IsFinite(localPoint.x) || !float.IsFinite(localPoint.y))
            {
                throw new ArgumentOutOfRangeException(nameof(localPoint));
            }

            _source = source;
            _sourcePoint = localPoint;
        }

        public void ResetSourcePoint()
        {
            _source = null;
            _sourcePoint = default;
        }

        public void StopEmitting()
        {
            if (_pending)
            {
                Clear();
                return;
            }

            _pending = false;
            _runtime?.StopEmitting();
            CheckCompletion();
            _coordinator?.Wake();
        }

        public void Pause()
        {
            _explicitlyPaused = IsPlaying;
            _runtime?.Pause();
        }

        public void Resume()
        {
            _explicitlyPaused = false;
            _runtime?.Resume();
            _coordinator?.Wake();
        }

        public new void Clear()
        {
            var hadParticles = LiveCount != 0;
            _requested = false;
            _pending = false;
            _completionPending = false;
            _arrivalsPending = 0;
            _explicitlyPaused = false;
            _hiddenPaused = false;
            ReleaseStorage();
            if (hadParticles)
            {
                MarkDirtyRepaint();
            }
        }

        // Editor previews call this explicitly. Player panels are advanced by their coordinator instead.
        public void Advance(double elapsed)
        {
            Tick(elapsed);
            DispatchCompletion();
        }

        internal void Tick(double elapsed)
        {
            if (!double.IsFinite(elapsed) || elapsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            }

            if (!NeedsUpdate || panel == null)
            {
                return;
            }

            if (_runtime != null && _runtime.HasFlights && (_target?.panel != panel || _runtime.Revision != _effect.Revision))
            {
                Clear();
                return;
            }

            // UI Toolkit transforms retained vertices without regenerating them. Panel-space vertices need a new inverse conversion.
            if (_simulationSpace == UIParticleSpace.Panel && LiveCount != 0 && _renderTransform != worldTransform)
            {
                _renderTransform = worldTransform;
                MarkDirtyRepaint();
            }

            var hidden = IsHidden();
            if (hidden && _hiddenBehaviour == UIParticleHiddenBehaviour.StopAndClear)
            {
                Clear();
                return;
            }

            if (_explicitlyPaused || _playbackSpeed == 0)
            {
                return;
            }

            var wasHiddenPaused = _hiddenPaused;
            _hiddenPaused = hidden && _hiddenBehaviour == UIParticleHiddenBehaviour.Pause;
            if (_hiddenPaused || !TryGetTransforms(out var spawn, out _, _pending || _runtime?.IsEmitting == true))
            {
                return;
            }

            if (wasHiddenPaused)
            {
                elapsed = 0;
            }

            if (_pending)
            {
                if (!TryPrepare(_effect))
                {
                    Clear();
                    LastPlayResult = ParticlePlayResult.BudgetExceeded;
                    return;
                }

                _runtime.Play(_seed);
                _pending = false;
                LastPlayResult = ParticlePlayResult.Started;
                elapsed = 0;
            }

            if (_runtime == null)
            {
                return;
            }

            var target = float2.zero;
            if (_runtime.HasFlights)
            {
                using (TargetMarker.Auto())
                {
                    target = (Vector2)_target.worldTransform.MultiplyPoint3x4(_targetPoint);
                }

                if (!math.all(math.isfinite(target)))
                {
                    Clear();
                    return;
                }
            }

            var hadParticles = _runtime.LiveCount != 0;
            if (_runtime.Advance(elapsed * _playbackSpeed, ref spawn, target) && !hidden && (hadParticles || _runtime.LiveCount != 0))
            {
                MarkDirtyRepaint();
            }

            _arrivalsPending += Math.Min(_runtime.TakeArrivals(), int.MaxValue - _arrivalsPending);
            CheckCompletion();
        }

        internal void DispatchCompletion()
        {
            var arrivals = _arrivalsPending;
            _arrivalsPending = 0;
            if (arrivals != 0)
            {
                Arrived?.Invoke(arrivals);
            }

            if (!_completionPending)
            {
                return;
            }

            _completionPending = false;
            Completed?.Invoke();
        }

        internal void Release()
        {
            Clear();
            Target = null;
            ResetSourcePoint();
            _ancestry.Clear();
        }

        private void CheckCompletion()
        {
            if (_requested && !_pending && _runtime != null && !_runtime.IsPlaying)
            {
                _requested = false;
                _completionPending = true;
            }
        }

        private void RestartRequested()
        {
            if (_requested)
            {
                Play();
            }
        }

        private bool IsHidden()
        {
            var rebuild = _ancestry.Count == 0;
            for (var i = 0; i < _ancestry.Count && !rebuild; i++)
            {
                var parent = i + 1 < _ancestry.Count ? _ancestry[i + 1] : null;
                rebuild = _ancestry[i].parent != parent;
            }

            if (rebuild)
            {
                _ancestry.Clear();
                for (var element = (VisualElement)this; element != null; element = element.parent)
                {
                    _ancestry.Add(element);
                }
            }

            foreach (var element in _ancestry)
            {
                if (element.resolvedStyle.display == DisplayStyle.None || element.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetTransforms(out float4x4 spawn, out float4x4 render, bool includeSource)
        {
            spawn = float4x4.identity;
            render = float4x4.identity;
            if (_pending && (!float.IsFinite(layout.width) || !float.IsFinite(layout.height) ||
                layout.width <= 0 || layout.height <= 0))
            {
                return false;
            }

            var world = worldTransform;
            var determinant = (world.m00 * world.m11) - (world.m01 * world.m10);
            if (!float.IsFinite(determinant) || Mathf.Abs(determinant) < 0.000001f)
            {
                return false;
            }

            var sourceWorld = world;
            if (includeSource && _source != null)
            {
                // A source removed independently is a documented stale coordinate request; suspend until replaced.
                if (_source.panel != panel)
                {
                    return false;
                }

                sourceWorld = _source.worldTransform * Matrix4x4.Translate(_sourcePoint);
                determinant = (sourceWorld.m00 * sourceWorld.m11) - (sourceWorld.m01 * sourceWorld.m10);
                if (!float.IsFinite(determinant) || Mathf.Abs(determinant) < 0.000001f)
                {
                    return false;
                }
            }

            if (_simulationSpace == UIParticleSpace.Panel)
            {
                spawn = sourceWorld;
                render = world.inverse;
            }
            else if (includeSource && _source != null)
            {
                spawn = world.inverse * sourceWorld;
            }

            return true;
        }

        private void Attach(AttachToPanelEvent evt)
        {
            _coordinator = UIParticleCoordinator.Register(evt.destinationPanel, this);
            if (_effectsEnabled && _playOnAttach && Application.isPlaying)
            {
                Play();
            }
            else if (_requested)
            {
                _coordinator.Wake();
            }
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            ReleaseVisualGeneration();
        }

        internal void ReleaseVisualGeneration()
        {
            Release();
            _coordinator?.Unregister(this);
            _coordinator = null;
            Completed = null;
            Arrived = null;
        }

        private void GeometryChanged(GeometryChangedEvent evt)
        {
            if (_pending)
            {
                Advance(0);
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            if (_runtime == null || _runtime.LiveCount == 0 || IsHidden() || !TryGetTransforms(out _, out var render, false))
            {
                return;
            }

            _runtime.PrepareMesh(new float4(_tint.r, _tint.g, _tint.b, _tint.a));
            _runtime.Draw(context, ref render, _simulationSpace == UIParticleSpace.Panel);
            _renderTransform = worldTransform;
        }
    }
}
