// <copyright file="IAnchorNavigationScreen.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    /// <summary>
    /// Interface for a navigation screen. A navigation screen is a screen that can be navigated to and from.
    /// </summary>
    public interface IAnchorNavigationScreen
    {
        /// <summary>
        /// Called when the navigation controller enters a destination.
        /// </summary>
        /// <param name="args"> The arguments passed to the destination.</param>
        void OnEnter(AnchorNavArgument[] args);

        /// <summary>
        /// Called when the navigation controller exits a destination.
        /// </summary>
        /// <param name="args"> The arguments passed to the destination.</param>
        void OnExit(AnchorNavArgument[] args);
    }
}
