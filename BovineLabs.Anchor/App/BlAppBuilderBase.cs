// <copyright file="BlAppBuilderBase.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class BlAppBuilderBase : BlAppBuilderBase<BLApp>
    {
        protected override void OnConfiguringApp(IServiceCollection services)
        {
            base.OnConfiguringApp(services);
            services.AddSingleton<IAssetService, AssetService>();
        }
    }

    /// <summary>
    /// A MonoBehaviour that can be used to build and host an app in a UIDocument.
    /// <para />
    /// This class is intended to be used as a base class for a MonoBehaviour that is attached to a GameObject in a scene.
    /// </summary>
    /// <typeparam name="T"> The type of the app to build. It is expected that this type is a subclass of <see cref="App"/>. </typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public class BlAppBuilderBase<T> : MonoBehaviour
        where T : BLApp
    {
        /// <summary> The UIDocument to host the app in. </summary>
        [Tooltip("The UIDocument to host the app in.")]
        public UIDocument UIDocument;

        private AppUIServiceCollection serviceCollection;

        protected virtual Type StoreService { get; } = typeof(StoreService);

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        /// <summary>
        /// Called when the app has been initialized.
        /// </summary>
        /// <param name="app"> The app that was initialized. </param>
        protected virtual void OnAppInitialized(T app)
        {
        }

        /// <summary>
        /// Called when the app is shutting down.
        /// </summary>
        /// <param name="app"> The app that is shutting down. </param>
        protected virtual void OnAppShuttingDown(T app)
        {
        }

        /// <summary>
        /// Called when the app builder is being configured.
        /// </summary>
        /// <param name="services"> The app services. </param>
        protected virtual void OnConfiguringApp(IServiceCollection services)
        {
            services.AddSingleton(typeof(IStoreService), this.StoreService);
            services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
        }

        protected virtual void RegisterViews(IServiceCollection services)
        {
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
                    services.AddTransient(view);
                }
                else
                {
                    services.AddSingleton(view);
                }
            }

            // Register all view models
            foreach (var viewModel in Core.GetAllImplementations<IViewModel>())
            {
                if (viewModel.GetCustomAttribute<TransientAttribute>() != null)
                {
                    services.AddTransient(viewModel);
                }
                else
                {
                    services.AddSingleton(viewModel);
                }
            }
        }

        private void OnEnable()
        {
            if (!this.UIDocument)
            {
                Debug.LogWarning("No UIDocument assigned to Program component. Aborting App startup.");

                return;
            }

            this.serviceCollection = new AppUIServiceCollection();

            var serviceProvider = new BlServiceProvider(this.serviceCollection);

            serviceProvider.AddServiceInstance<IServiceProvider, BlServiceProvider>(serviceProvider);
            serviceProvider.AddServiceInstance<GameObject, GameObject>(this.gameObject);
            serviceProvider.AddServiceInstance<ViewProvider, ViewProvider>(new ViewProvider(serviceProvider));

            this.serviceCollection.TryAddSingleton<IApp, T>();

            this.OnConfiguringApp(this.serviceCollection);
            this.RegisterViews(this.serviceCollection);

            var host = new UIToolkitHost(this.UIDocument);

            this.serviceCollection.AddSingleton<IPanelService, PanelService>(); // Panels last so everything can be setup beforehand
            this.serviceCollection.IsReadOnly = true;

            var app = serviceProvider.GetRequiredService<IApp>();
            app.Initialize(serviceProvider, host);

            this.OnAppInitialized((T)app);
        }

        private void OnDisable()
        {
            if (App.current is not T app)
            {
                return;
            }

            this.OnAppShuttingDown(app);
            if (this.UIDocument)
            {
                this.UIDocument.rootVisualElement?.Clear();
            }

            app.Shutdown();
            app.Dispose();
        }

        private void Reset()
        {
            this.UIDocument = this.GetComponent<UIDocument>();
        }
    }
}
