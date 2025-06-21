// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class AnchorNavHost : NavHost
    {
        public AnchorNavHost()
        {
            this.pickingMode = PickingMode.Ignore;
            this.Q<StackView>().pickingMode = PickingMode.Ignore;
        }
    }
}