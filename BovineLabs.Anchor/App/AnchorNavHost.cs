// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using Unity.AppUI.Navigation;

    public class AnchorNavHost : NavHost
    {
        public AnchorNavHost()
        {
            this.makeScreen = this.MakeScreen;
        }

        private NavigationScreen MakeScreen(Type screenType)
        {
            try
            {
                return (NavigationScreen)AnchorApp.current.services.GetService(screenType);
            }
            catch (Exception)
            {
                return (NavigationScreen)Activator.CreateInstance(screenType);
            }
        }
    }
}
