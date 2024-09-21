// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Logging;
    using UnityEngine.UIElements;

    public class AnchorApp : App
    {
        private static readonly FunctionPointer<NavigateDelegate> NavigateFunction =
            new(Marshal.GetFunctionPointerForDelegate<NavigateDelegate>(NavigateForwarding));

        private delegate void NavigateDelegate(in FixedString64Bytes screen);

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Overwriting AppUI standard")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Overwriting AppUI standard")]
        public static new AnchorApp current => App.current as AnchorApp;

        public virtual Panel Panel => (Panel)this.rootVisualElement;

        public NavController Controller { get; private set; }

        public NavGraphViewAsset GraphViewAsset { get; set; }

        public VisualElement PopupContainer { get; private set; }

        public VisualElement NotificationContainer { get; private set; }

        public VisualElement TooltipContainer { get; private set; }

        public virtual INavVisualController NavVisualController { get; internal set; }

        public virtual void Initialize()
        {
            Burst.Static.Data = NavigateFunction;

            var toolbarView = this.services.GetService<ToolbarView>();
            this.rootVisualElement.Add(toolbarView);

            var navigationView = this.services.GetService<NavigationView>();
            this.rootVisualElement.Add(navigationView);
            this.Controller = navigationView.Controller;

            this.PopupContainer = this.rootVisualElement.Q<VisualElement>("popup-container");
            this.NotificationContainer = this.rootVisualElement.Q<VisualElement>("notification-container");
            this.TooltipContainer = this.rootVisualElement.Q<VisualElement>("tooltip-container");
        }

        /// <summary> A burst compatible way to Navigate to a new screen in the <see cref="GraphViewAsset"/>.  </summary>
        /// <param name="screen"> The screen to navigate to. </param>
        public static void Navigate(in FixedString64Bytes screen)
        {
            if (Burst.Static.Data.IsCreated)
            {
                Burst.Static.Data.Invoke(screen);
            }
        }

        /// <summary> Navigate to a new screen in the <see cref="GraphViewAsset"/>. </summary>
        /// <param name="screen"> The screen to navigate to. </param>
        public void Navigate(string screen)
        {
            if (!this.Controller.Navigate(screen))
            {
                Log.Warning($"Tried to navigate to {screen} but it wasn't found");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnPropertyChangedDelegate))]
        private static void NavigateForwarding(in FixedString64Bytes screen)
        {
            current.Navigate(screen.ToString());
        }

        private static class Burst
        {
            public static readonly SharedStatic<FunctionPointer<NavigateDelegate>> Static =
                SharedStatic<FunctionPointer<NavigateDelegate>>.GetOrCreate<AnchorApp, FunctionPointer<NavigateDelegate>>();
        }
    }
}
