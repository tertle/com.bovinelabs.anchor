// <copyright file="AnchorAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
#if BL_CORE
    using BovineLabs.Core.Utility;
#endif
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using UnityEngine.UIElements;

    public class AnchorAppBuilder : AnchorAppBuilder<AnchorApp>
    {
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public abstract class AnchorAppBuilder<T> : UIToolkitAppBuilder<T>
        where T : AnchorApp
    {
#if BL_CORE
#if !UNITY_EDITOR && !BL_DEBUG
        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;
#endif
        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;
#else
        [field: SerializeField]
        [field: Tooltip("If true, will disable instantiation in builds without toolbar to speed up initialization.")]
        private bool ToolbarOnly { get; set; }

        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;

        protected NavGraphViewAsset NavigationGraph => this.navigationGraph;
#endif

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

#if BL_CORE
        protected virtual Type UXMLService { get; } = typeof(UXMLService);
#else
        protected virtual Type UXMLService { get; }
#endif

        protected virtual Type NavHost { get; } = typeof(AnchorNavHost);

        protected virtual Type NavVisualController => null;

        /// <inheritdoc />
        protected override void OnConfiguringApp(AppBuilder builder)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif
            base.OnConfiguringApp(builder);

            builder.services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            builder.services.AddSingleton(typeof(IViewModelService), this.ViewModelService);
            builder.services.AddSingleton(typeof(AnchorNavHost), this.NavHost);

            if (this.UXMLService != null)
            {
                builder.services.AddSingleton(typeof(IUXMLService), this.UXMLService);
            }

            if (this.NavVisualController != null)
            {
                builder.services.AddSingleton(typeof(INavVisualController), this.NavVisualController);
            }

            // Register all services
#if BL_CORE
            foreach (var services in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
#else
            foreach (var services in Core.GetAllWithAttribute<IsServiceAttribute>())
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

        /// <inheritdoc/>
        protected override void OnAppInitialized(T app)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif
            base.OnAppInitialized(app);

#if UNITY_EDITOR || BL_DEBUG
            foreach (var style in this.DebugStyleSheets)
            {
                app.rootVisualElement.styleSheets.Add(style);
            }
#endif

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
