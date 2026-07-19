// <copyright file="AnchorAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Default app builder that scaffolds an <see cref="AnchorApp"/> with the standard Anchor services.
    /// </summary>
    public class AnchorAppBuilder : AnchorAppBuilder<AnchorApp>
    {
    }

    /// <summary>
    /// <para>A MonoBehaviour that can be used to build and host an app in a UI Toolkit panel component.</para>
    /// <para>Uses a <see cref="PanelRenderer"/> to host the app root.</para>
    /// <para>This class is intended to be used as a base class for a MonoBehaviour that is attached to a GameObject in a scene.</para>
    /// </summary>
    /// <typeparam name="T">The type of the app to build.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    [RequireComponent(typeof(PanelRenderer))]
    public abstract class AnchorAppBuilder<T> : MonoBehaviour
        where T : AnchorApp, new()
    {
        private PanelRenderer panelRenderer;

        private AnchorServiceProvider serviceProvider;
        private T anchorApp;
        private VisualElement hostRootVisualElement;
        private VisualElement appRootVisualElement;
        private IAnchorNavHostReloadState pendingNavigationState;
        private bool visualGenerationActive;
        private int lastPanelVersion = -1;

        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type AudioService { get; } = typeof(AudioService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        /// <summary>
        /// Gets the panel type used for the app root.
        /// </summary>
        /// <remarks>
        /// The type must implement <see cref="IAnchorPanel"/> and provide a public parameterless constructor.
        /// </remarks>
        protected virtual Type PanelType { get; } = typeof(AnchorPanel);

        private void Awake()
        {
            this.panelRenderer = this.GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            this.hostRootVisualElement = null;
            this.panelRenderer.RegisterUIReloadCallback(this.OnPanelRendererReload);
            ((IPanelComponent)this.panelRenderer).PerformValidation(true);
        }

        private void OnDisable()
        {
            this.panelRenderer.UnregisterUIReloadCallback(this.OnPanelRendererReload);
            this.lastPanelVersion = -1;

            try
            {
                this.ReleaseCurrentVisualGeneration();
            }
            finally
            {
                this.hostRootVisualElement = null;
            }
        }

        private void OnDestroy()
        {
            this.panelRenderer.UnregisterUIReloadCallback(this.OnPanelRendererReload);
            this.ShutdownApp();
        }

        private void Update()
        {
            this.anchorApp?.Update();
        }

        protected virtual void OnConfigureServices(AnchorServiceCollection services)
        {
            services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            services.AddSingleton(typeof(IViewModelService), this.ViewModelService);
            services.AddSingleton(typeof(IAudioService), this.AudioService);

            if (this.UXMLService != null)
            {
                services.AddSingleton(typeof(IUXMLService), this.UXMLService);
            }

            // Register all services
            foreach (var service in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
            {
                var isTransient = service.GetCustomAttribute<TransientAttribute>() != null;
                if (isTransient)
                {
                    services.AddTransient(service);
                }
                else
                {
                    services.AddSingleton(service);
                }

                if (!isTransient && typeof(IAnchorToolbarHost).IsAssignableFrom(service))
                {
                    services.AddAlias(typeof(IAnchorToolbarHost), service);
                }
            }
        }

        /// <summary>
        /// Creates the panel host for the app root.
        /// </summary>
        /// <returns>Panel implementation used by this app instance.</returns>
        protected virtual IAnchorPanel CreatePanel()
        {
            var panelType = this.PanelType ?? typeof(AnchorPanel);
            if (!typeof(IAnchorPanel).IsAssignableFrom(panelType))
            {
                throw new InvalidOperationException(
                    $"{nameof(this.PanelType)} '{panelType.FullName}' must implement {nameof(IAnchorPanel)}.");
            }

            if (panelType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(this.PanelType)} '{panelType.FullName}' must have a public parameterless constructor.");
            }

            return (IAnchorPanel)Activator.CreateInstance(panelType);
        }

        protected virtual void OnAppInitialized(T app)
        {
        }

        protected virtual void OnVisualGenerationInitialized(T app)
        {
            if (this.ToolbarOnly)
            {
                app.InitializeToolbar();
                return;
            }

#if UNITY_EDITOR || BL_DEBUG
            foreach (var style in this.DebugStyleSheets)
            {
                app.RootVisualElement.styleSheets.Add(style);
            }
#endif

            app.Initialize();
            app.InitializeToolbar();
        }

        protected virtual void OnVisualGenerationShuttingDown(T app)
        {
        }

        protected virtual void OnAppShuttingDown(T app)
        {
        }

        private void InitializeApp()
        {
            var services = new AnchorServiceCollection();
            this.OnConfigureServices(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.anchorApp = new T();

            try
            {
                this.anchorApp.Initialize(this.serviceProvider);
                this.OnAppInitialized(this.anchorApp);
            }
            catch
            {
                this.anchorApp.Dispose();
                this.serviceProvider.Dispose();
                this.anchorApp = null;
                this.serviceProvider = null;
                throw;
            }
        }

        private void InitializeVisualGeneration(IAnchorNavHostReloadState navigationState)
        {
            var panel = this.CreatePanel();
            var root = panel.RootVisualElement ??
                throw new InvalidOperationException($"{panel.GetType().FullName} returned a null root visual element.");

            this.appRootVisualElement = root;

            try
            {
                root.pickingMode = PickingMode.Ignore;
                this.anchorApp.SetPanel(panel);
                this.AttachAppRootToHost();
                this.visualGenerationActive = true;
                this.OnVisualGenerationInitialized(this.anchorApp);

                if (navigationState != null)
                {
                    if (this.anchorApp.NavHost == null)
                    {
                        throw new InvalidOperationException("The visual generation did not initialize a navigation host.");
                    }

                    this.anchorApp.NavHost.RestoreReloadState(navigationState);
                }

                this.anchorApp.RefreshScreenMetrics();
            }
            catch
            {
                this.ReleaseVisualGeneration();
                throw;
            }
        }

        private void ShutdownApp()
        {
            try
            {
                if (this.anchorApp != null)
                {
                    try
                    {
                        this.InvokeVisualGenerationShuttingDown();
                    }
                    finally
                    {
                        try
                        {
                            this.OnAppShuttingDown(this.anchorApp);
                        }
                        finally
                        {
                            this.appRootVisualElement?.RemoveFromHierarchy();
                            this.anchorApp.Dispose();
                        }
                    }
                }
            }
            finally
            {
                this.serviceProvider?.Dispose();

                this.anchorApp = null;
                this.serviceProvider = null;
                this.appRootVisualElement = null;
                this.hostRootVisualElement = null;
                this.pendingNavigationState = null;
                this.visualGenerationActive = false;
            }
        }

        private void ReleaseVisualGeneration()
        {
            try
            {
                this.InvokeVisualGenerationShuttingDown();
            }
            finally
            {
                try
                {
                    this.appRootVisualElement?.RemoveFromHierarchy();
                    this.anchorApp.ReleaseVisualGeneration();
                }
                finally
                {
                    this.appRootVisualElement = null;
                }
            }
        }

        private void ReleaseCurrentVisualGeneration()
        {
            if (this.anchorApp?.RootVisualElement == null)
            {
                return;
            }

            this.CaptureNavigationState();
            this.ReleaseVisualGeneration();
        }

        private void CaptureNavigationState()
        {
            if (this.ToolbarOnly || this.pendingNavigationState != null)
            {
                return;
            }

            var navHost = this.anchorApp.NavHost ??
                throw new InvalidOperationException("The current visual generation does not have a navigation host.");
            this.pendingNavigationState = navHost.CaptureReloadState() ??
                throw new InvalidOperationException($"{navHost.GetType().FullName} returned a null navigation reload state.");
        }

        private void InvokeVisualGenerationShuttingDown()
        {
            if (!this.visualGenerationActive)
            {
                return;
            }

            this.visualGenerationActive = false;
            this.OnVisualGenerationShuttingDown(this.anchorApp);
        }

        private void AttachAppRootToHost()
        {
            if (this.hostRootVisualElement == null || this.appRootVisualElement == null)
            {
                return;
            }

            this.hostRootVisualElement.Clear();
            this.hostRootVisualElement.Add(this.appRootVisualElement);
        }

        private void OnPanelRendererReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            if (version == this.lastPanelVersion)
            {
                return;
            }

            this.hostRootVisualElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));

            try
            {
                if (this.anchorApp == null)
                {
                    this.InitializeApp();
                }

                this.ReleaseCurrentVisualGeneration();

                this.InitializeVisualGeneration(this.pendingNavigationState);
                this.pendingNavigationState = null;
                this.lastPanelVersion = version;
            }
            catch (Exception ex)
            {
                BLGlobalLogger.LogErrorString($"Failed to build Anchor panel visual generation {version}: {ex}");
                throw;
            }
        }
    }
}
