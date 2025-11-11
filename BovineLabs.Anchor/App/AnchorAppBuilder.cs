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
    using BovineLabs.Core.Utility;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
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
    /// <typeparam name="T"> The type of the app to build. It is expected that this type is a subclass of <see cref="App"/>. </typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Base implementation")]
    public abstract class AnchorAppBuilder<T> : UIToolkitAppBuilder<T>
        where T : AnchorApp
    {
        private AnchorNavHost.SavedState state;

        protected bool ToolbarOnly => AnchorSettings.I.ToolbarOnly;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        /// <inheritdoc/>
        protected override void OnConfiguringApp(AppBuilder builder)
        {
#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif
            builder.services.AddSingleton(typeof(ILocalStorageService), this.LocalStorageService);
            builder.services.AddSingleton(typeof(IViewModelService), this.ViewModelService);

            if (this.UXMLService != null)
            {
                builder.services.AddSingleton(typeof(IUXMLService), this.UXMLService);
            }

            // Register all services
            foreach (var services in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
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
            base.OnAppInitialized(app);

#if !UNITY_EDITOR && !BL_DEBUG
            if (this.ToolbarOnly)
            {
                return;
            }
#endif

#if UNITY_EDITOR || BL_DEBUG
            foreach (var style in this.DebugStyleSheets)
            {
                app.rootVisualElement.styleSheets.Add(style);
            }
#endif

            app.Initialize();

            if (this.state != null)
            {
                app.NavHost.RestoreState(this.state);
            }
        }

        /// <inheritdoc/>
        protected override void OnAppShuttingDown(T app)
        {
            base.OnAppShuttingDown(app);

            this.state = app.NavHost.SaveState();
        }

        private void Reset()
        {
            this.uiDocument = this.GetComponent<UIDocument>();
        }
    }
}
