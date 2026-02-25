// <copyright file="AnchorAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BovineLabs.Anchor.DependencyInjection;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using Unity.AppUI.UI;
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
        protected UIDocument uiDocument;

        private AnchorNavHostSaveState state;
        private AnchorServiceProvider serviceProvider;
        private T app;

        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        private void OnEnable()
        {
            if (this.uiDocument == null)
            {
                BLGlobalLogger.LogWarningString($"No {nameof(UIDocument)} assigned to {nameof(AnchorAppBuilder<T>)}. Aborting app startup.");
                return;
            }

            var services = new AnchorServiceCollection();
            this.OnConfigureServices(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.app = new T();

            var root = new Panel();
            this.uiDocument.rootVisualElement?.Clear();
            this.uiDocument.rootVisualElement?.Add(root);

            this.app.Initialize(this.serviceProvider, root);
            this.OnAppInitialized(this.app);
        }

        private void OnDisable()
        {
            if (this.app == null)
            {
                return;
            }

            this.OnAppShuttingDown(this.app);

            if (this.uiDocument != null)
            {
                this.uiDocument.rootVisualElement?.Clear();
            }

            this.app.Shutdown();
            this.app.Dispose();
            this.serviceProvider?.Dispose();

            this.app = null;
            this.serviceProvider = null;
        }

        protected virtual void OnConfigureServices(AnchorServiceCollection services)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif
            services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            services.AddSingleton(typeof(IViewModelService), this.ViewModelService);

            if (this.UXMLService != null)
            {
                services.AddSingleton(typeof(IUXMLService), this.UXMLService);
            }

            // Register all services
            foreach (var service in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
            {
                if (service.GetCustomAttribute<TransientAttribute>() != null)
                {
                    services.AddTransient(service);
                }
                else
                {
                    services.AddSingleton(service);
                }
            }
        }

        protected virtual void OnAppInitialized(T app)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif

#if UNITY_EDITOR || BL_DEBUG
            foreach (var style in this.DebugStyleSheets)
            {
                app.RootVisualElement.styleSheets.Add(style);
            }
#endif

            app.Initialize();

            if (this.state != null)
            {
                app.NavHost.RestoreState(this.state);
            }
        }

        protected virtual void OnAppShuttingDown(T app)
        {
            this.state = app.NavHost.SaveState();
        }

        private void Reset()
        {
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
