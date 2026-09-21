#if UNITY_INCLUDE_INSTRUMENTATION
#define CUSTOM_SAFE_AREA
#endif

namespace BovineLabs.Anchor
{
    using System;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core;
    using BovineLabs.Core.ConfigVars;
    using JetBrains.Annotations;
    using Unity.Burst;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [UsedImplicitly]
    [Configurable]
    public class AnchorApp : IDisposable
    {
        public const string DefaultServiceTabName = "Service";

#if CUSTOM_SAFE_AREA
        [ConfigVar("anchor.safe-area", 0, 0, 0, 0,
            "Custom SafeArea for testing. This is not a rect but instead offsets from each edge so will work on any resolution.")]
        private static readonly SharedStatic<Vector4> CustomSafeArea = SharedStatic<Vector4>.GetOrCreate<AnchorApp, SafeAreaType>();
#endif

        private bool _disposed;
        private bool _hasScreenMetrics;
        private AnchorScreenMetrics _lastScreenMetrics;
        private IAnchorToolbarHost _toolbarHost;
        private string _theme;
        private string _scale;

        internal bool RestoringNavigationState { get; set; }

        public event Action<AnchorScreenMetrics> ScreenMetricsChanged;

        [NoAutoStaticsCleanup]
        public static AnchorApp Current { get; private set; }

        public static Rect SafeArea => GetSafeArea();

        public IAnchorPanel Panel { get; private set; }

        public IServiceProvider Services { get; private set; }

        public VisualElement RootVisualElement { get; private set; }

        public string Theme
        {
            get => Panel?.Theme ?? _theme;
            set
            {
                _theme = value;

                if (Panel != null)
                {
                    Panel.Theme = value;
                }
            }
        }

        public string Scale
        {
            get => Panel?.Scale ?? _scale;
            set
            {
                _scale = value;

                if (Panel != null)
                {
                    Panel.Scale = value;
                }
            }
        }

        public virtual string ServiceTabName => DefaultServiceTabName;

        public IAnchorNavHost NavHost { get; set; }

        public VisualElement PopupContainer { get; private set; }

        public VisualElement NotificationContainer { get; private set; }

        public VisualElement TooltipContainer { get; private set; }

        internal void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorApp));
            }

            SetCurrentApp(this);
            Services = provider;
        }

        internal void SetPanel(IAnchorPanel panel)
        {
            if (Services == null)
            {
                throw new InvalidOperationException($"{nameof(AnchorApp)} must be initialized before assigning a panel.");
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorApp));
            }

            Panel = panel ?? throw new ArgumentNullException(nameof(panel));
            RootVisualElement = panel.RootVisualElement ??
                throw new InvalidOperationException($"{panel.GetType().FullName} returned a null root visual element.");

            if (_theme == null)
            {
                _theme = panel.Theme;
            }
            else
            {
                panel.Theme = _theme;
            }

            if (_scale == null)
            {
                _scale = panel.Scale;
            }
            else
            {
                panel.Scale = _scale;
            }

            _hasScreenMetrics = false;
            _lastScreenMetrics = default;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ReleaseVisualGeneration();
            _toolbarHost = null;
            Services = null;
            _theme = null;
            _scale = null;
            ScreenMetricsChanged = null;

            if (ReferenceEquals(Current, this))
            {
                SetCurrentApp(null);
            }

            _disposed = true;
        }

        private static Rect GetSafeArea()
        {
#if CUSTOM_SAFE_AREA
            var safeArea = CustomSafeArea.Data;
            if (!safeArea.Equals(Vector4.zero))
            {
                return new Rect(safeArea.x, safeArea.y, Screen.width - safeArea.x - safeArea.z, Screen.height - safeArea.y - safeArea.w);
            }
#endif
            return Screen.safeArea;
        }

        public virtual void Initialize()
        {
            RootVisualElement.pickingMode = PickingMode.Ignore;

            var navHost = new AnchorNavHost(AnchorSettings.I.Actions, AnchorSettings.I.Animations);
            NavHost = navHost;
            RootVisualElement.Add(navHost);

            if (!RestoringNavigationState && !string.IsNullOrWhiteSpace(AnchorSettings.I.StartDestination))
            {
                NavHost.Navigate(AnchorSettings.I.StartDestination, new AnchorNavOptions());
            }

            PopupContainer = RootVisualElement.Q<VisualElement>("popup-container");
            NotificationContainer = RootVisualElement.Q<VisualElement>("notification-container");
            TooltipContainer = RootVisualElement.Q<VisualElement>("tooltip-container");
        }

        public void InitializeToolbar()
        {
            if (RootVisualElement == null)
            {
                throw new InvalidOperationException("A panel must be assigned before initializing the toolbar.");
            }

            _toolbarHost ??= Services.GetService(typeof(IAnchorToolbarHost)) as IAnchorToolbarHost;
            if (_toolbarHost != null)
            {
                RootVisualElement.Insert(0, _toolbarHost.CreateRootVisualElement());
            }
        }

        internal void Update()
        {
            if (Panel != null)
            {
                _theme = Panel.Theme;
                _scale = Panel.Scale;
            }

            if (RootVisualElement == null)
            {
                return;
            }

            UpdateScreenMetrics(AnchorScreenMetrics.Current());
        }

        internal void RefreshScreenMetrics()
        {
            _hasScreenMetrics = false;
            UpdateScreenMetrics(AnchorScreenMetrics.Current());
        }

        internal void ReleaseVisualGeneration()
        {
            RootVisualElement?.Query<AnchorParticles>().ForEach(static particles => particles.ReleaseVisualGeneration());

            if (Panel != null)
            {
                _theme = Panel.Theme;
                _scale = Panel.Scale;
            }

            try
            {
                _toolbarHost?.ReleaseRootVisualElement();
            }
            finally
            {
                PopupContainer = null;
                NotificationContainer = null;
                TooltipContainer = null;
                NavHost = null;
                Panel = null;
                RootVisualElement = null;
                _hasScreenMetrics = false;
                _lastScreenMetrics = default;
            }
        }

        internal bool UpdateScreenMetrics(AnchorScreenMetrics metrics)
        {
            if (RootVisualElement == null)
            {
                return false;
            }

            if (_hasScreenMetrics && _lastScreenMetrics.Equals(metrics))
            {
                return false;
            }

            _hasScreenMetrics = true;
            _lastScreenMetrics = metrics;
            ScreenMetricsChanged?.Invoke(metrics);
            return true;
        }

        private static void SetCurrentApp(AnchorApp app)
        {
            if (app != null && Current != null && !ReferenceEquals(Current, app))
            {
                BLGlobalLogger.LogError($"An {nameof(AnchorApp)} has already been initialized, replacing it.");
                Current.Dispose();
            }

            Current = app;
        }

#if CUSTOM_SAFE_AREA
        private struct SafeAreaType
        {
        }
#endif
    }
}
