// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_EDITOR || BL_DEBUG
#define CUSTOM_SAFE_AREA
#endif

namespace BovineLabs.Anchor
{
    using System;
    using BovineLabs.Anchor.DependencyInjection;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.ConfigVars;
    using JetBrains.Annotations;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Root Anchor application that wires the AppUI navigation stack, toolbar integration, and burst-safe helpers.
    /// </summary>
    [UsedImplicitly]
    [Configurable]
    public class AnchorApp : IDisposable
    {
        /// <summary>The default name for the service tab exposed in the toolbar.</summary>
        public const string DefaultServiceTabName = "Service";

#if CUSTOM_SAFE_AREA
        [ConfigVar("anchor.safe-area", 0, 0, 0, 0, "Custom SafeArea for testing. This is not a rect but instead offsets from each edge so will work on any resolution.")]
        private static readonly SharedStatic<Vector4> CustomSafeArea = SharedStatic<Vector4>.GetOrCreate<ToolbarView, SafeAreaType>();
#endif

        private bool disposed;

        /// <summary>Gets the currently running <see cref="AnchorApp"/>.</summary>
        public static AnchorApp Current { get; private set; }

        /// <summary>Event called when the app is shutting down.</summary>
        public static event Action ShuttingDown;

        public static Rect SafeArea => GetSafeArea();

        /// <summary>Gets the AppUI panel that hosts the Anchor visual tree.</summary>
        public virtual Panel Panel => (Panel)this.RootVisualElement;

        /// <summary>Gets the current app service provider.</summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>Gets the root visual element hosting the app content.</summary>
        public VisualElement RootVisualElement { get; private set; }

        /// <summary>Gets the name used for the default service tab added to the toolbar.</summary>
        public virtual string ServiceTabName => DefaultServiceTabName;

        /// <summary>Gets or sets the navigation host that manages screen transitions.</summary>
        public AnchorNavHost NavHost { get; set; }

        /// <summary>Gets the container that holds popup visual elements instantiated by the app.</summary>
        public VisualElement PopupContainer { get; private set; }

        /// <summary>Gets the container that displays notification visuals.</summary>
        public VisualElement NotificationContainer { get; private set; }

        /// <summary>Gets the container that manages tooltip content.</summary>
        public VisualElement TooltipContainer { get; private set; }

        internal void Initialize(IServiceProvider provider, VisualElement root)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            SetCurrentApp(this);
            this.Services = provider;
            this.RootVisualElement = root;
        }

        internal void Shutdown()
        {
            ShuttingDown?.Invoke();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.PopupContainer = null;
            this.NotificationContainer = null;
            this.TooltipContainer = null;
            this.NavHost = null;
            this.RootVisualElement = null;
            this.Services = null;

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
            this.Panel.pickingMode = PickingMode.Ignore;

#if BL_DEBUG || UNITY_EDITOR
            var toolbarView = this.Services.GetRequiredService<ToolbarView>();
            this.RootVisualElement.Add(toolbarView);
#endif

            this.NavHost = new AnchorNavHost(AnchorSettings.I.Actions, AnchorSettings.I.Animations);
            if (!string.IsNullOrWhiteSpace(AnchorSettings.I.StartDestination))
            {
                this.NavHost.Navigate(AnchorSettings.I.StartDestination, new AnchorNavOptions());
            }

            this.RootVisualElement.Add(this.NavHost);

            this.PopupContainer = this.RootVisualElement.Q<VisualElement>("popup-container");
            this.NotificationContainer = this.RootVisualElement.Q<VisualElement>("notification-container");
            this.TooltipContainer = this.RootVisualElement.Q<VisualElement>("tooltip-container");
        }

#if CUSTOM_SAFE_AREA
        private struct SafeAreaType
        {
        }
#endif

        private static void SetCurrentApp(AnchorApp app)
        {
            if (app != null && Current != null && !ReferenceEquals(Current, app))
            {
                throw new InvalidOperationException($"An {nameof(AnchorApp)} has already been initialized.");
            }

            Current = app;
        }
    }
}
