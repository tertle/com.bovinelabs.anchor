// <copyright file="AnchorAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Services;
#if BL_CORE
    using BovineLabs.Core.Utility;
#endif
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorAppBuilder : AnchorAppBuilder<AnchorApp>
    {
        [SerializeField]
        private NavGraphViewAsset navigationGraph;

        protected override NavGraphViewAsset NavigationGraph => this.navigationGraph;
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public abstract class AnchorAppBuilder<T> : UIToolkitAppBuilder<T>
        where T : AnchorApp
    {
#if UNITY_EDITOR || BL_DEBUG
        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();
#endif

        [SerializeField]
        [Tooltip("If true, will disable instantiation in builds without toolbar to speed up initialization.")]
        private bool toolbarOnly;

        protected abstract NavGraphViewAsset NavigationGraph { get; }

        /// <summary> Gets the optional <see cref="IStoreService"/> type. If not set, IStoreService will not be registered. </summary>
        protected virtual Type StoreService => null;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type GameView { get; } = typeof(NavigationView);

        protected virtual Type NavHost { get; } = typeof(AnchorNavHost);

        protected virtual Type NavVisualController => null;

        /// <inheritdoc />
        protected override void OnConfiguringApp(AppBuilder builder)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.toolbarOnly)
            {
                return;
            }
#endif
            base.OnConfiguringApp(builder);

            if (this.StoreService != null)
            {
                builder.services.AddSingleton(typeof(IStoreService), this.StoreService);
            }

            builder.services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            builder.services.AddSingleton(typeof(IViewModelService), this.ViewModelService);
            builder.services.AddSingleton(typeof(AnchorNavHost), this.NavHost);

            if (this.NavVisualController != null)
            {
                builder.services.AddSingleton(typeof(INavVisualController), this.NavVisualController);
            }

            var s = ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>().ToArray();

            // Register all services
#if BL_CORE
            foreach (var services in s)
#else
            foreach (var view in Core.GetAllWithAttribute<IView>())
#endif
            {
                if (services.GetCustomAttribute<TransientAttribute>() != null)
                {
                    builder.services.AddTransient(services);
                }
                else
                {
                    builder.services.AddSingleton(services);
                }
            }
        }

        protected override void OnAppInitialized(T app)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.toolbarOnly)
            {
                return;
            }
#endif
            base.OnAppInitialized(app);

#if UNITY_EDITOR || BL_DEBUG
            foreach (var style in this.debugStyleSheets)
            {
                app.rootVisualElement.styleSheets.Add(style);
            }
#endif

            app.GraphViewAsset = this.NavigationGraph;

            if (this.NavVisualController != null)
            {
                app.NavVisualController = app.services.GetRequiredService<INavVisualController>();
            }

            app.Initialize();
        }

        private void Reset()
        {
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
