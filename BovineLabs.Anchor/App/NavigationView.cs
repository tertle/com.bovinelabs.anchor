// <copyright file="NavigationView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.Navigation;
    using Unity.Burst;
    using Unity.Collections;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class NavigationView : VisualElement, IView
    {
        public static readonly FunctionPointer<NavigateDelegate> Navigation;
        private static NavController controller;

        static NavigationView()
        {
            Navigation = new FunctionPointer<NavigateDelegate>(Marshal.GetFunctionPointerForDelegate<NavigateDelegate>(NavigateForwarding));
        }

        public NavigationView()
        {
            if (AnchorApp.current.GraphViewAsset == null)
            {
                this.style.display = DisplayStyle.None;
                return;
            }

            this.style.flexGrow = 1;

            var navHost = new AnchorNavHost();
            navHost.navController.SetGraph(AnchorApp.current.GraphViewAsset);
            navHost.visualController = AnchorApp.current.NavVisualController;
            this.Add(navHost);
            navHost.StretchToParentSize();

            controller = navHost.navController;
        }

        public delegate void NavigateDelegate(in FixedString64Bytes screen);

        public int Priority => 0;

        [AOT.MonoPInvokeCallback(typeof(OnPropertyChangedDelegate))]
        private static void NavigateForwarding(in FixedString64Bytes screen)
        {
            controller?.Navigate(screen.ToString());
        }
    }
}
