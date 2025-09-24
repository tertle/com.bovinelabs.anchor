// <copyright file="AnchorNavOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.AppUI.Navigation;
    using UnityEngine;

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
    [Serializable]
    public class AnchorNavOptions
    {
        [SerializeField]
        private AnchorStackStrategy stackStrategy;

        [SerializeField]
        private string popUpToDestination;

        [SerializeField]
        private bool popUpToInclusive;

        [SerializeField]
        private NavigationAnimation enterAnim;

        [SerializeField]
        private NavigationAnimation exitAnim;

        [SerializeField]
        private NavigationAnimation popEnterAnim;

        [SerializeField]
        private NavigationAnimation popExitAnim;

        [SerializeField]
        private bool popUpToSaveState;

        [SerializeField]
        private bool restoreState;

        [SerializeField]
        private bool launchSingleTop;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavOptions"/> class.
        /// </summary>
        public AnchorNavOptions()
        {
            this.enterAnim = NavigationAnimation.None;
            this.exitAnim = NavigationAnimation.None;
            this.popEnterAnim = NavigationAnimation.None;
            this.popExitAnim = NavigationAnimation.None;
        }

        /// <summary>
        /// Gets or sets strategy for popping up to a destination.
        /// </summary>
        public AnchorStackStrategy StackStrategy
        {
            get => this.stackStrategy;
            set => this.stackStrategy = value;
        }

        /// <summary>
        /// Gets or sets route for the destination to pop up to before navigating.
        /// When set, all non-matching destinations should be popped from the back stack.
        /// </summary>
        public string PopUpToDestination
        {
            get => this.popUpToDestination;
            set => this.popUpToDestination = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether the target destination in PopUpTo should be popped from the back stack.
        /// </summary>
        public bool PopUpToInclusive
        {
            get => this.popUpToInclusive;
            set => this.popUpToInclusive = value;
        }

        /// <summary>
        /// Gets or sets the animation to use when navigating to the destination.
        /// </summary>
        public NavigationAnimation EnterAnim
        {
            get => this.enterAnim;
            set => this.enterAnim = value;
        }

        /// <summary>
        /// Gets or sets the animation to use when navigating away from the destination.
        /// </summary>
        public NavigationAnimation ExitAnim
        {
            get => this.exitAnim;
            set => this.exitAnim = value;
        }

        /// <summary>
        /// Gets or sets the custom enter Animation/Animator that should be run when this destination is popped from the back stack.
        /// </summary>
        public NavigationAnimation PopEnterAnim
        {
            get => this.popEnterAnim;
            set => this.popEnterAnim = value;
        }

        /// <summary>
        /// Gets or sets the custom exit Animation/Animator that should be run when this destination is popped from the back stack.
        /// </summary>
        public NavigationAnimation PopExitAnim
        {
            get => this.popExitAnim;
            set => this.popExitAnim = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether the back stack and the state of all destinations between the current destination
        /// and popUpToId should be saved for later restoration
        /// </summary>
        public bool PopUpToSaveState
        {
            get => this.popUpToSaveState;
            set => this.popUpToSaveState = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether this navigation action should restore any state previously saved by
        /// Builder.setPopUpTo or the popUpToSaveState attribute.
        /// </summary>
        public bool RestoreState
        {
            get => this.restoreState;
            set => this.restoreState = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether this navigation action should launch as single-top
        /// (i.e., there will be at most one copy of a given destination on the top of the back stack).
        /// </summary>
        public bool LaunchSingleTop
        {
            get => this.launchSingleTop;
            set => this.launchSingleTop = value;
        }
    }
}
