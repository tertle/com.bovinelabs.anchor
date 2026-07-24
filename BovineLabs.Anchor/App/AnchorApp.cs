// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_INCLUDE_INSTRUMENTATION
#define CUSTOM_SAFE_AREA
#endif

namespace BovineLabs.Anchor
{
    using System;
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

    /// <summary>
    /// Root Anchor application that wires the navigation stack, toolbar integration, and Burst-safe helpers.
    /// </summary>
    [Preserve]
    [UsedImplicitly]
    [Configurable]
    public class AnchorApp : IDisposable
    {
        /// <summary>The default name for the service tab exposed in the toolbar.</summary>
        public const string DefaultServiceTabName = "Service";

#if CUSTOM_SAFE_AREA
        [ConfigVar("anchor.safe-area", 0, 0, 0, 0,
            "Custom SafeArea for testing. This is not a rect but instead offsets from each edge so will work on any resolution.")]
        private static readonly SharedStatic<Vector4> CustomSafeArea = SharedStatic<Vector4>.GetOrCreate<AnchorApp, SafeAreaType>();
#endif

        private bool disposed;
        private bool hasScreenMetrics;
        private AnchorScreenMetrics lastScreenMetrics;
        private IAnchorToolbarHost toolbarHost;
        private string theme;
        private string scale;

        /// <summary>
        /// Event raised when the app detects screen-space metric changes such as size or safe-area updates.
        /// </summary>
        public event Action<AnchorScreenMetrics> ScreenMetricsChanged;

        /// <summary>Gets the currently running <see cref="AnchorApp"/>.</summary>
        [NoAutoStaticsCleanup]
        public static AnchorApp Current { get; private set; }

        public static Rect SafeArea => GetSafeArea();

        /// <summary>Gets the app panel host abstraction.</summary>
        public IAnchorPanel Panel { get; private set; }

        /// <summary>Gets the current app service provider.</summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>Gets the root visual element hosting the app content.</summary>
        public VisualElement RootVisualElement { get; private set; }

        /// <summary>Gets or sets the theme retained across visual generations.</summary>
        public string Theme
        {
            get => this.Panel?.Theme ?? this.theme;
            set
            {
                this.theme = value;

                if (this.Panel != null)
                {
                    this.Panel.Theme = value;
                }
            }
        }

        /// <summary>Gets or sets the scale retained across visual generations.</summary>
        public string Scale
        {
            get => this.Panel?.Scale ?? this.scale;
            set
            {
                this.scale = value;

                if (this.Panel != null)
                {
                    this.Panel.Scale = value;
                }
            }
        }

        /// <summary>Gets the name used for the default service tab added to the toolbar.</summary>
        public virtual string ServiceTabName => DefaultServiceTabName;

        /// <summary>Gets or sets the navigation host that manages screen transitions.</summary>
        public IAnchorNavHost NavHost { get; set; }

        /// <summary>Gets the container that holds popup visual elements instantiated by the app.</summary>
        public VisualElement PopupContainer { get; private set; }

        /// <summary>Gets the container that displays notification visuals.</summary>
        public VisualElement NotificationContainer { get; private set; }

        /// <summary>Gets the container that manages tooltip content.</summary>
        public VisualElement TooltipContainer { get; private set; }

        internal void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorApp));
            }

            SetCurrentApp(this);
            this.Services = provider;
        }

        internal void SetPanel(IAnchorPanel panel)
        {
            if (this.Services == null)
            {
                throw new InvalidOperationException($"{nameof(AnchorApp)} must be initialized before assigning a panel.");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorApp));
            }

            this.Panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.RootVisualElement = panel.RootVisualElement ??
                throw new InvalidOperationException($"{panel.GetType().FullName} returned a null root visual element.");

            if (this.theme == null)
            {
                this.theme = panel.Theme;
            }
            else
            {
                panel.Theme = this.theme;
            }

            if (this.scale == null)
            {
                this.scale = panel.Scale;
            }
            else
            {
                panel.Scale = this.scale;
            }

            this.hasScreenMetrics = false;
            this.lastScreenMetrics = default;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.ReleaseVisualGeneration();
            this.toolbarHost = null;
            this.Services = null;
            this.theme = null;
            this.scale = null;
            this.ScreenMetricsChanged = null;

            if (ReferenceEquals(Current, this))
            {
                SetCurrentApp(null);
            }

            this.disposed = true;
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

        /// <summary>
        /// Configures the root visual tree, initializes the navigation host, and registers toolbars and containers.
        /// </summary>
        public virtual void Initialize()
        {
            this.RootVisualElement.pickingMode = PickingMode.Ignore;

            var navHost = new AnchorNavHost(AnchorSettings.I.Actions, AnchorSettings.I.Animations);
            this.NavHost = navHost;
            this.RootVisualElement.Add(navHost);

            if (!string.IsNullOrWhiteSpace(AnchorSettings.I.StartDestination))
            {
                this.NavHost.Navigate(AnchorSettings.I.StartDestination, new AnchorNavOptions());
            }

            this.PopupContainer = this.RootVisualElement.Q<VisualElement>("popup-container");
            this.NotificationContainer = this.RootVisualElement.Q<VisualElement>("notification-container");
            this.TooltipContainer = this.RootVisualElement.Q<VisualElement>("tooltip-container");
        }

        public void InitializeToolbar()
        {
            if (this.RootVisualElement == null)
            {
                throw new InvalidOperationException("A panel must be assigned before initializing the toolbar.");
            }

            this.toolbarHost ??= this.Services.GetService(typeof(IAnchorToolbarHost)) as IAnchorToolbarHost;
            if (this.toolbarHost != null)
            {
                this.RootVisualElement.Insert(0, this.toolbarHost.CreateRootVisualElement());
            }
        }

        internal void Update()
        {
            if (this.Panel != null)
            {
                this.theme = this.Panel.Theme;
                this.scale = this.Panel.Scale;
            }

            if (this.RootVisualElement == null)
            {
                return;
            }

            this.UpdateScreenMetrics(AnchorScreenMetrics.Current());
        }

        internal void RefreshScreenMetrics()
        {
            this.hasScreenMetrics = false;
            this.UpdateScreenMetrics(AnchorScreenMetrics.Current());
        }

        internal void ReleaseVisualGeneration()
        {
            if (this.Panel != null)
            {
                this.theme = this.Panel.Theme;
                this.scale = this.Panel.Scale;
            }

            try
            {
                this.toolbarHost?.ReleaseRootVisualElement();
            }
            finally
            {
                this.PopupContainer = null;
                this.NotificationContainer = null;
                this.TooltipContainer = null;
                this.NavHost = null;
                this.Panel = null;
                this.RootVisualElement = null;
                this.hasScreenMetrics = false;
                this.lastScreenMetrics = default;
            }
        }

        internal bool UpdateScreenMetrics(AnchorScreenMetrics metrics)
        {
            if (this.RootVisualElement == null)
            {
                return false;
            }

            if (this.hasScreenMetrics && this.lastScreenMetrics.Equals(metrics))
            {
                return false;
            }

            this.hasScreenMetrics = true;
            this.lastScreenMetrics = metrics;
            this.ScreenMetricsChanged?.Invoke(metrics);
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
