// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_EDITOR || BL_DEBUG
#define CUSTOM_SAFE_AREA
#endif

namespace BovineLabs.Anchor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.ConfigVars;
    using JetBrains.Annotations;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Root Anchor application that wires the AppUI navigation stack, toolbar integration, and burst-safe helpers.
    /// </summary>
    [UsedImplicitly]
    [Configurable]
    public class AnchorApp : App
    {
        /// <summary>The default name for the service tab exposed in the toolbar.</summary>
        public const string DefaultServiceTabName = "Service";

#if CUSTOM_SAFE_AREA
        [ConfigVar("anchor.safe-area", 0, 0, 0, 0, "Custom SafeArea for testing. This is not a rect but instead offsets from each edge so will work on any resolution.")]
        private static readonly SharedStatic<Vector4> CustomSafeArea = SharedStatic<Vector4>.GetOrCreate<ToolbarView, SafeAreaType>();
#endif

        /// <summary>Gets the strongly typed instance of the currently running <see cref="AnchorApp"/>.</summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "AppUI standard")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "AppUI standard")]
        public static new AnchorApp current => App.current as AnchorApp;

        public static Rect SafeArea => GetSafeArea();

        /// <summary>Gets the AppUI panel that hosts the Anchor visual tree.</summary>
        public virtual Panel Panel => (Panel)this.rootVisualElement;

        /// <summary>Gets the name used for the default service tab added to the toolbar.</summary>
        public virtual string ServiceTabName => DefaultServiceTabName;

        /// <summary>Gets or sets the navigation host that manages screen transitions.</summary>
        public AnchorNavHost NavHost { get; set; }

        /// <summary>Gets or sets the navigation graph asset that describes available destinations.</summary>
        public NavGraphViewAsset GraphViewAsset { get; set; }

        /// <summary>Gets the container that holds popup visual elements instantiated by the app.</summary>
        public VisualElement PopupContainer { get; private set; }

        /// <summary>Gets the container that displays notification visuals.</summary>
        public VisualElement NotificationContainer { get; private set; }

        /// <summary>Gets the container that manages tooltip content.</summary>
        public VisualElement TooltipContainer { get; private set; }

        private static Rect GetSafeArea()
        {
#if CUSTOM_SAFE_AREA
            var safeArea = CustomSafeArea.Data;
            if (!safeArea.Equals(Vector4.zero))
            {
                return new Rect(safeArea.x, safeArea.y, Screen.width - safeArea.x - safeArea.z, Screen.height - safeArea.y - safeArea.w);
            }
#endif
            return Screen.safeArea;
        }

        private static (T Method, FunctionPointer<T> Function) CreateDelegate<T>(T method)
        {
            return (method, new FunctionPointer<T>(Marshal.GetFunctionPointerForDelegate(method)));
        }

        /// <summary>
        /// Configures the root visual tree, initializes the navigation host, and registers toolbars and containers.
        /// </summary>
        public virtual void Initialize()
        {
            this.Panel.pickingMode = PickingMode.Ignore;

#if BL_DEBUG || UNITY_EDITOR
            var toolbarView = this.services.GetRequiredService<ToolbarView>();
            this.rootVisualElement.Add(toolbarView);
#endif

            this.NavHost = new AnchorNavHost(AnchorSettings.I.Actions);
            if (!string.IsNullOrWhiteSpace(AnchorSettings.I.StartDestination))
            {
                this.NavHost.Navigate(AnchorSettings.I.StartDestination, new AnchorNavOptions());
            }

            this.rootVisualElement.Add(this.NavHost);

            this.PopupContainer = this.rootVisualElement.Q<VisualElement>("popup-container");
            this.NotificationContainer = this.rootVisualElement.Q<VisualElement>("notification-container");
            this.TooltipContainer = this.rootVisualElement.Q<VisualElement>("tooltip-container");
        }

        /// <summary> This has been disabled in favour of overriding <see cref="Initialize" />. </summary>
        public sealed override void InitializeComponent()
        {
            base.InitializeComponent();
        }

#if CUSTOM_SAFE_AREA
        private struct SafeAreaType
        {
        }
#endif
    }
}
