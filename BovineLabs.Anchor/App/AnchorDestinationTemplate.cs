// <copyright file="AnchorDestinationTemplate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using Unity.AppUI.Navigation;
    using UnityEngine;

    /// <summary> Uses get service to support injecting instead of Activator. </summary>
    [Serializable]
    public class AnchorDestinationTemplate : DefaultDestinationTemplate
    {
        /// <summary> Initializes a new instance of the <see cref="AnchorDestinationTemplate"/> class. </summary>
        public AnchorDestinationTemplate()
        {
            // Turn off all the defaults
            this.showBottomNavBar = false;
            this.showAppBar = false;
            this.showBackButton = false;
            this.showDrawer = false;
            this.showNavigationRail = false;
        }

        /// <inheritdoc/>
        public override INavigationScreen CreateScreen(NavHost host)
        {
            NavigationScreen screen;
            var type = Type.GetType(this.template);
            if (type == null)
            {
                var msg = string.IsNullOrEmpty(this.template) ? "The template type is not set." : $"The template type '{this.template}' is not valid.";
                Debug.LogWarning($"{msg} Falling back to default screen type.");
                screen = new NavigationScreen();
            }
            else
            {
                screen = AnchorApp.current.services.GetService(type) as NavigationScreen;
            }

            if (screen == null)
            {
                throw new InvalidOperationException($"The template '{this.template}' could not be instantiated. " +
                    "Ensure that the type is a valid NavigationScreen and is accessible " +
                    "and has a parameterless constructor.");
            }

            screen.showBottomNavBar = this.showBottomNavBar;
            screen.showAppBar = this.showAppBar;
            screen.showBackButton = this.showBackButton;
            screen.showDrawer = this.showDrawer;
            screen.showNavigationRail = this.showNavigationRail;

            return screen;
        }
    }
}
