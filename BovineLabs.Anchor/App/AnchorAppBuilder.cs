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
    /// <para>Use PanelRenderer on Unity 6.5+ and <see cref="UIDocument"/> on earlier Unity 6 releases.</para>
    /// <para>This class is intended to be used as a base class for a MonoBehaviour that is attached to a GameObject in a scene.</para>
    /// </summary>
    /// <typeparam name="T">The type of the app to build.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public abstract class AnchorAppBuilder<T> : MonoBehaviour
        where T : AnchorApp, new()
    {
#if UNITY_6000_5_OR_NEWER
        [SerializeField]
        private PanelRenderer panelRenderer;
#else
        [SerializeField]
        private UIDocument uiDocument;
#endif

        private AnchorNavHostSaveState state;
        private AnchorServiceProvider serviceProvider;
        private T anchorApp;
        private VisualElement hostRootVisualElement;
        private VisualElement appRootVisualElement;

        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        /// <summary>
        /// Gets the panel type used for the app root.
        /// </summary>
        /// <remarks>
        /// The type must implement <see cref="IAnchorPanel"/> and provide a public parameterless constructor.
        /// </remarks>
        protected virtual Type PanelType { get; } = typeof(AnchorPanel);

        internal void OnEnable()
        {
            if (!this.TryBindHost())
            {
#if UNITY_6000_5_OR_NEWER
                BLGlobalLogger.LogWarningString($"No {nameof(PanelRenderer)} assigned to {nameof(AnchorAppBuilder<T>)}. Aborting app startup.");
#else
                BLGlobalLogger.LogWarningString($"No {nameof(UIDocument)} assigned to {nameof(AnchorAppBuilder<T>)}. Aborting app startup.");
#endif
                return;
            }

            var services = new AnchorServiceCollection();
            this.OnConfigureServices(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.anchorApp = new T();

            var panel = this.CreatePanel();
            this.appRootVisualElement = panel.RootVisualElement;
            this.appRootVisualElement.pickingMode = PickingMode.Ignore;
            this.AttachAppRootToHost();

            this.anchorApp.Initialize(this.serviceProvider, panel);
            this.OnAppInitialized(this.anchorApp);
        }

        internal void OnDisable()
        {
#if UNITY_6000_5_OR_NEWER
            this.panelRenderer?.UnregisterUIReloadCallback(this.OnPanelRendererReload);
#endif

            if (this.anchorApp == null)
            {
                this.hostRootVisualElement = null;
                this.appRootVisualElement = null;
                return;
            }

            this.OnAppShuttingDown(this.anchorApp);

            this.hostRootVisualElement?.Clear();

            this.anchorApp.Dispose();
            this.serviceProvider?.Dispose();

            this.anchorApp = null;
            this.serviceProvider = null;
            this.hostRootVisualElement = null;
            this.appRootVisualElement = null;
        }

        internal void Update()
        {
            this.anchorApp?.Update();
        }

        protected virtual void OnConfigureServices(AnchorServiceCollection services)
        {
            services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            services.AddSingleton(typeof(IViewModelService), this.ViewModelService);

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

            if (this.state != null)
            {
                app.NavHost.RestoreState(this.state);
            }
        }

        protected virtual void OnAppShuttingDown(T app)
        {
            if (!this.ToolbarOnly)
            {
                this.state = app.NavHost.SaveState();
            }
        }

        private void Reset()
        {
#if UNITY_6000_5_OR_NEWER
            this.panelRenderer = this.GetComponent<PanelRenderer>();
#else
            this.uiDocument = this.GetComponent<UIDocument>();
#endif
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

        private bool TryBindHost()
        {
            this.hostRootVisualElement = null;

#if UNITY_6000_5_OR_NEWER
            this.panelRenderer ??= this.GetComponent<PanelRenderer>();

            if (this.panelRenderer != null)
            {
                this.panelRenderer.RegisterUIReloadCallback(this.OnPanelRendererReload);
                ((IPanelComponent)this.panelRenderer).PerformValidation(true);
                return true;
            }
#else
            this.uiDocument ??= this.GetComponent<UIDocument>();
            this.hostRootVisualElement = this.uiDocument?.rootVisualElement;
            return this.uiDocument != null;
#endif

            return false;
        }

#if UNITY_6000_5_OR_NEWER
        private void OnPanelRendererReload(PanelRenderer _, VisualElement rootElement)
        {
            this.hostRootVisualElement = rootElement;
            this.AttachAppRootToHost();
        }
#endif
    }
}
