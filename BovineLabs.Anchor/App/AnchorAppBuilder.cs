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
    /// <para>A MonoBehaviour that can be used to build and host an app in a UIDocument.</para>
    /// <para>This class is intended to be used as a base class for a MonoBehaviour that is attached to a GameObject in a scene.</para>
    /// </summary>
    /// <typeparam name="T">The type of the app to build.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public abstract class AnchorAppBuilder<T> : MonoBehaviour
        where T : AnchorApp, new()
    {
        [SerializeField]
        private UIDocument uiDocument;

        private AnchorNavHostSaveState state;
        private AnchorServiceProvider serviceProvider;
        private T anchorApp;

        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        protected UIDocument Document => this.uiDocument;

        /// <summary>
        /// Gets the panel type used for the app root.
        /// </summary>
        /// <remarks>
        /// The type must implement <see cref="IAnchorPanel"/> and provide a public parameterless constructor.
        /// </remarks>
        protected virtual Type PanelType { get; } = typeof(AnchorPanel);

        internal void OnEnable()
        {
            if (this.uiDocument == null)
            {
                BLGlobalLogger.LogWarningString($"No {nameof(UIDocument)} assigned to {nameof(AnchorAppBuilder<T>)}. Aborting app startup.");
                return;
            }

            var services = new AnchorServiceCollection();
            this.OnConfigureServices(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.anchorApp = new T();

            var panel = this.CreatePanel();
            panel.RootVisualElement.pickingMode = PickingMode.Ignore;
            var root = panel.RootVisualElement;
            this.uiDocument.rootVisualElement?.Clear();
            this.uiDocument.rootVisualElement?.Add(root);

            this.anchorApp.Initialize(this.serviceProvider, panel);
            this.OnAppInitialized(this.anchorApp);
        }

        internal void OnDisable()
        {
            if (this.anchorApp == null)
            {
                return;
            }

            this.OnAppShuttingDown(this.anchorApp);

            if (this.uiDocument != null)
            {
                this.uiDocument.rootVisualElement?.Clear();
            }

            this.anchorApp.Dispose();
            this.serviceProvider?.Dispose();

            this.anchorApp = null;
            this.serviceProvider = null;
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
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
