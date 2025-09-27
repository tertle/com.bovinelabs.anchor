// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using AOT;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Toolbar;
    using JetBrains.Annotations;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Collections;
    using UnityEngine.UIElements;

    [UsedImplicitly]
    public class AnchorApp : App
    {
        public const string DefaultServiceTabName = "Service";

        private static readonly FunctionPointer<NavigateDelegate> NavigateFunction;
        private static readonly FunctionPointer<CurrentDelegate> CurrentFunction;

        [UsedImplicitly]
        private static object navigateFunctionGCPrevention;
        [UsedImplicitly]
        private static object currentFunctionGCPrevention;

        static AnchorApp()
        {
            (navigateFunctionGCPrevention, NavigateFunction) = CreateDelegate<NavigateDelegate>(NavigateForwarding);
            (currentFunctionGCPrevention, CurrentFunction) = CreateDelegate<CurrentDelegate>(CurrentForwarding);
        }

        private delegate void NavigateDelegate(in FixedString32Bytes screen);

        private delegate void CurrentDelegate(out FixedString32Bytes name);

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "AppUI standard")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "AppUI standard")]
        public static new AnchorApp current => App.current as AnchorApp;

        public virtual Panel Panel => (Panel)this.rootVisualElement;

        public virtual string ServiceTabName => DefaultServiceTabName;

        public AnchorNavHost NavHost { get; set; }

        public NavGraphViewAsset GraphViewAsset { get; set; }

        public VisualElement PopupContainer { get; private set; }

        public VisualElement NotificationContainer { get; private set; }

        public VisualElement TooltipContainer { get; private set; }

        private static (T Method, FunctionPointer<T> Function) CreateDelegate<T>(T method)
        {
            return (method, new FunctionPointer<T>(Marshal.GetFunctionPointerForDelegate(method)));
        }

        public virtual void Initialize()
        {
            Burst.NavigateFunc.Data = NavigateFunction;
            Burst.CurrentFunc.Data = CurrentFunction;

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

        /// <summary> A burst compatible way to Navigate to a new screen in the <see cref="GraphViewAsset" />. </summary>
        /// <param name="screen"> The screen to navigate to. </param>
        public static void Navigate(in FixedString32Bytes screen)
        {
            if (Burst.NavigateFunc.Data.IsCreated)
            {
                Burst.NavigateFunc.Data.Invoke(screen);
            }
        }

        /// <summary> A burst compatible way to get the <see cref="NavController.currentDestination"/> from the <see cref="NavController"/>. </summary>
        /// <returns>The name of the current destination, or default if null.</returns>
        public static FixedString32Bytes CurrentDestination()
        {
            if (Burst.CurrentFunc.Data.IsCreated)
            {
                Burst.CurrentFunc.Data.Invoke(out var name);
                return name;
            }

            return default;
        }

        // TODO this seems pointless with navhsot
        /// <summary> Navigate to a new screen in the <see cref="GraphViewAsset" />. </summary>
        /// <param name="screen"> The screen to navigate to. </param>
        public void Navigate(string screen)
        {
            this.NavHost.Navigate(screen);
        }

        /// <summary> This has been disabled in favour of overriding <see cref="Initialize" />. </summary>
        public sealed override void InitializeComponent()
        {
            base.InitializeComponent();
        }

        [MonoPInvokeCallback(typeof(NavigateDelegate))]
        private static void NavigateForwarding(in FixedString32Bytes screen)
        {
            current.Navigate(screen.ToString());
        }

        [MonoPInvokeCallback(typeof(CurrentDelegate))]
        private static void CurrentForwarding(out FixedString32Bytes name)
        {
            name = current.NavHost.CurrentDestination ?? default(FixedString32Bytes);
        }

        private static class Burst
        {
            public static readonly SharedStatic<FunctionPointer<NavigateDelegate>> NavigateFunc =
                SharedStatic<FunctionPointer<NavigateDelegate>>.GetOrCreate<AnchorApp, FunctionPointer<NavigateDelegate>>();

            public static readonly SharedStatic<FunctionPointer<CurrentDelegate>> CurrentFunc =
                SharedStatic<FunctionPointer<CurrentDelegate>>.GetOrCreate<AnchorApp, FunctionPointer<CurrentDelegate>>();
        }
    }
}
