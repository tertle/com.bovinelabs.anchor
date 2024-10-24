// <copyright file="NavigationView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.Navigation;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class NavigationView : VisualElement, IView
    {
        public NavigationView()
        {
            this.pickingMode = PickingMode.Ignore;

            var navHost = new AnchorNavHost();
            this.Add(navHost);

            navHost.navController.SetGraph(AnchorApp.current.GraphViewAsset);
            navHost.visualController = AnchorApp.current.NavVisualController;
            this.Controller = navHost.navController;

            if (AnchorApp.current.GraphViewAsset == null)
            {
                this.style.display = DisplayStyle.None;
                return;
            }

            this.style.flexGrow = 1;
            navHost.StretchToParentSize();
        }

        public NavController Controller { get; }
    }
}
