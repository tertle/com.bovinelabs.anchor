// <copyright file="BlUIAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class BLUIAppBuilder : BlUIAppBuilderBase<BLApp>
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
    public class BlUIAppBuilderBase<T> : MonoBehaviour
        where T : BLApp
    {
        /// <summary> The UIDocument to host the app in. </summary>
        [Tooltip("The UIDocument to host the app in.")]
        public UIDocument UIDocument;

        public StyleSheet[] StyleSheets = Array.Empty<StyleSheet>();

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

                services.AddTransient(view);
            }

            // Register all view models
            foreach (var viewModels in Core.GetAllImplementations<IViewModel>())
            {
                services.AddTransient(viewModels);
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
            this.serviceCollection.AddSingleton<Panel>();

            var serviceProvider = new BlServiceProvider(this.serviceCollection);

            serviceProvider.AddServiceInstance<IServiceProvider, BlServiceProvider>(serviceProvider);
            serviceProvider.AddServiceInstance<GameObject, GameObject>(this.gameObject);

            this.serviceCollection.TryAddSingleton<IApp, T>();

            this.OnConfiguringApp(this.serviceCollection);
            this.RegisterViews(this.serviceCollection);

            this.serviceCollection.AddSingleton<IPanelService, PanelService>(); // Panels last so everything can be setup beforehand
            this.serviceCollection.IsReadOnly = true;

            var host = new UIToolkitHost(this.UIDocument);
            var app = serviceProvider.GetRequiredService<IApp>();
            app.Initialize(serviceProvider, host);

            var tapp = (T)app;

            foreach (var stylesheet in this.StyleSheets)
            {
                if (stylesheet == null)
                {
                    continue;
                }

                tapp.Panel.styleSheets.Add(stylesheet);
            }

            this.OnAppInitialized(tapp);
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
