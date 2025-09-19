// <copyright file="AnchorNavOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.AppUI.Navigation;

    /// <summary>
    /// Strategy for popping up to a destination.
    /// </summary>
    public enum AnchorStackStrategy
    {
        /// <summary> The back stack will not be popped. </summary>
        None,

        /// <summary> The back stack will be popped up to the destination with the given ID. </summary>
        PopToSpecificDestination,

        /// <summary>
        /// The back stack will be popped up to the current graph's start destination.
        /// </summary>
        PopToRoot,
    }

    /// <summary>
    /// NavOptions stores special options for navigate actions
    /// </summary>
    public class AnchorNavOptions
    {
        /// <summary>
        /// Strategy for popping up to a destination.
        /// </summary>
        public AnchorStackStrategy StackStrategy { get; set; }

        /// <summary>
        /// Route for the destination to pop up to before navigating.
        /// When set, all non-matching destinations should be popped from the back stack.
        /// </summary>
        public string PopUpToDestination { get; set; }

        /// <summary>
        /// Whether the target destination in PopUpTo should be popped from the back stack.
        /// </summary>
        public bool PopUpToInclusive { get; set; }

        /// <summary>
        /// The animation to use when navigating to the destination.
        /// </summary>
        public NavigationAnimation EnterAnim { get; set; }

        /// <summary>
        /// The animation to use when navigating away from the destination.
        /// </summary>
        public NavigationAnimation ExitAnim { get; set; }

        /// <summary>
        /// The custom enter Animation/Animator that should be run when this destination is popped from the back stack.
        /// </summary>
        public NavigationAnimation PopEnterAnim { get; set; }

        /// <summary>
        /// The custom exit Animation/Animator that should be run when this destination is popped from the back stack.
        /// </summary>
        public NavigationAnimation PopExitAnim { get; set; }

        /// <summary>
        /// Whether the back stack and the state of all destinations between the current destination
        /// and popUpToId should be saved for later restoration
        /// </summary>
        public bool PopUpToSaveState { get; set; }

        /// <summary>
        /// Whether this navigation action should restore any state previously saved by
        /// Builder.setPopUpTo or the popUpToSaveState attribute.
        /// </summary>
        public bool RestoreState { get; set; }

        /// <summary>
        /// Whether this navigation action should launch as single-top
        /// (i.e., there will be at most one copy of a given destination on the top of the back stack).
        /// </summary>
        public bool LaunchSingleTop { get; set; }
    }
}
