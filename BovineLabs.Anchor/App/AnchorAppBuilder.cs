// <copyright file="AnchorAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorAppBuilder : AnchorAppBuilder<AnchorApp>
    {
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public class AnchorAppBuilder<T> : UIToolkitAppBuilder<T>
        where T : AnchorApp
    {
        [SerializeField]
        private NavGraphViewAsset navigationGraph;

        protected NavGraphViewAsset NavigationGraph => this.navigationGraph;

        protected virtual Type StoreService { get; } = typeof(StoreService);

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewService { get; } = typeof(ViewService);

        protected virtual Type GameView { get; } = typeof(NavigationView);

        protected virtual Type NavVisualController => null;

        /// <inheritdoc/>
        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);

            builder.services.AddSingleton(typeof(IStoreService), this.StoreService);
            builder.services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            builder.services.AddSingleton(typeof(IViewService), this.ViewService);

            if (this.NavVisualController != null)
            {
                builder.services.AddSingleton(typeof(INavVisualController), this.NavVisualController);
            }

            // Register all views
            foreach (var view in Core.GetAllImplementations<IView>())
            {
                if (!typeof(VisualElement).IsAssignableFrom(view))
                {
                    Debug.LogError("IView can only be assigned to VisualElements");
                    continue;
                }

                if (view.GetCustomAttribute<TransientAttribute>() != null)
                {
                    builder.services.AddTransient(view);
                }
                else
                {
                    builder.services.AddSingleton(view);
                }
            }

            // Register all view models
            foreach (var viewModel in Core.GetAllImplementations<IViewModel>())
            {
                if (viewModel.GetCustomAttribute<TransientAttribute>() != null)
                {
                    builder.services.AddTransient(viewModel);
                }
                else
                {
                    builder.services.AddSingleton(viewModel);
                }
            }
        }

        protected override void OnAppInitialized(T app)
        {
            base.OnAppInitialized(app);

            app.GraphViewAsset = this.navigationGraph;

            if (this.NavVisualController != null)
            {
                app.NavVisualController = app.services.GetService<INavVisualController>();
            }

            app.Initialize();
        }

        private void Reset()
        {
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
