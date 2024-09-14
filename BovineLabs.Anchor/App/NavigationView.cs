// <copyright file="NavigationView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class NavigationView : VisualElement, IViewRoot
    {
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
        }

        public int Priority => 0;
    }
}
