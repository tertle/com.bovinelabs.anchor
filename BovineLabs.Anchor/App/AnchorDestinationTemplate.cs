// <copyright file="AnchorDestinationTemplate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using BovineLabs.Core;
    using UnityEngine.UIElements;
    using UnityEngine;

    /// <summary>
    /// Configures how a destination screen is created and initialized from the Anchor service provider.
    /// </summary>
    [Serializable]
    public class AnchorDestinationTemplate
    {
        [SerializeField]
        protected string template;

        [SerializeField]
        protected bool showBottomNavBar;

        [SerializeField]
        protected bool showAppBar;

        [SerializeField]
        protected bool showBackButton;

        [SerializeField]
        protected bool showDrawer;

        [SerializeField]
        protected bool showNavigationRail;

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

        /// <summary>
        /// Creates a destination screen instance from the configured template type.
        /// </summary>
        /// <returns>The created destination screen.</returns>
        public virtual AnchorDestinationScreen CreateScreen()
        {
            AnchorDestinationScreen screen;
            var type = Type.GetType(this.template);
            if (type == null)
            {
                var msg = string.IsNullOrEmpty(this.template) ? "The template type is not set." : $"The template type '{this.template}' is not valid.";
                BLGlobalLogger.LogWarningString($"{msg} Falling back to default screen type.");
                screen = new AnchorDestinationScreen();
            }
            else
            {
                screen = AnchorApp.Current.Services.GetService(type) as AnchorDestinationScreen;
            }

            if (screen == null)
            {
                throw new InvalidOperationException($"The template '{this.template}' could not be instantiated. " +
                    $"Ensure that the type is a valid {nameof(AnchorDestinationScreen)} and is accessible " +
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

    /// <summary>
    /// Base destination element that exposes common chrome visibility flags.
    /// </summary>
    public class AnchorDestinationScreen : VisualElement
    {
        public bool showBottomNavBar { get; set; }

        public bool showAppBar { get; set; }

        public bool showBackButton { get; set; }

        public bool showDrawer { get; set; }

        public bool showNavigationRail { get; set; }
    }
}

