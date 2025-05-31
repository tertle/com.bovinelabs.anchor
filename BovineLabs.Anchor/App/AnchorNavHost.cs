// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorNavHost : NavHost
    {
        public AnchorNavHost()
        {
            this.pickingMode = PickingMode.Ignore;
            this.Q<StackView>().pickingMode = PickingMode.Ignore;

            this.makeScreen = this.MakeScreen;
        }

        private NavigationScreen MakeScreen(Type screenType)
        {
            try
            {
                return (NavigationScreen)AnchorApp.current.services.GetService(screenType);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }
    }
}
