// <copyright file="BlAppBuilder.cs" company="BovineLabs">
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

    public class BlAppBuilder : BlAppBuilder<BLApp>
    {
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public class BlAppBuilder<T> : UIToolkitAppBuilder<T>
        where T : BLApp
    {
        protected virtual Type StoreService { get; } = typeof(StoreService);

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        /// <inheritdoc/>
        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);

            builder.services.AddSingleton(typeof(IStoreService), this.StoreService);
            builder.services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            builder.services.AddSingleton<IPanelService, PanelService>();

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

        private void Reset()
        {
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
