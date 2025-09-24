// <copyright file="AnchorNavOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
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

    [Serializable]
    public enum AnchorPopupStrategy
    {
        /// <summary>No popup behaviour; navigation replaces the current destination.</summary>
        None,

        /// <summary>Overlay the destination on top of the current visual stack.</summary>
        PopupOnCurrent,

        /// <summary>Navigate to a base destination first (if required) before overlaying the popup.</summary>
        EnsureBaseAndPopup,
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

        [SerializeField]
        private AnchorPopupStrategy popupStrategy;

        [SerializeField]
        private string popupBaseDestination;

        [SerializeField]
        private List<Argument> popupBaseArguments = new();

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
        /// Gets or sets the strategy used to manipulate the back stack before navigating.
        /// </summary>
        public AnchorStackStrategy StackStrategy
        {
            get => this.stackStrategy;
            set => this.stackStrategy = value;
        }

        /// <summary>
        /// Gets or sets the destination route to pop up to before navigating. When provided, all other destinations above
        /// the target will be removed from the back stack.
        /// </summary>
        public string PopUpToDestination
        {
            get => this.popUpToDestination;
            set => this.popUpToDestination = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the destination specified by <see cref="PopUpToDestination"/> should
        /// also be removed from the back stack.
        /// </summary>
        public bool PopUpToInclusive
        {
            get => this.popUpToInclusive;
            set => this.popUpToInclusive = value;
        }

        /// <summary>
        /// Gets or sets the animation used when navigating to the destination.
        /// </summary>
        public NavigationAnimation EnterAnim
        {
            get => this.enterAnim;
            set => this.enterAnim = value;
        }

        /// <summary>
        /// Gets or sets the animation used when navigating away from the current destination.
        /// </summary>
        public NavigationAnimation ExitAnim
        {
            get => this.exitAnim;
            set => this.exitAnim = value;
        }

        /// <summary>
        /// Gets or sets the animation that plays when this destination re-enters the stack after a pop operation.
        /// </summary>
        public NavigationAnimation PopEnterAnim
        {
            get => this.popEnterAnim;
            set => this.popEnterAnim = value;
        }

        /// <summary>
        /// Gets or sets the animation that plays when this destination is popped from the stack.
        /// </summary>
        public NavigationAnimation PopExitAnim
        {
            get => this.popExitAnim;
            set => this.popExitAnim = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether destinations removed by <see cref="PopUpToDestination"/> should have
        /// their state preserved for later restoration.
        /// </summary>
        public bool PopUpToSaveState
        {
            get => this.popUpToSaveState;
            set => this.popUpToSaveState = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether previously saved state should be restored when navigating to this
        /// destination.
        /// </summary>
        public bool RestoreState
        {
            get => this.restoreState;
            set => this.restoreState = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether navigating to this destination should avoid pushing another instance
        /// if it is already at the top of the back stack.
        /// </summary>
        public bool LaunchSingleTop
        {
            get => this.launchSingleTop;
            set => this.launchSingleTop = value;
        }

        /// <summary>
        /// Gets or sets the popup presentation strategy to use for this navigation request.
        /// </summary>
        public AnchorPopupStrategy PopupStrategy
        {
            get => this.popupStrategy;
            set => this.popupStrategy = value;
        }

        /// <summary>
        /// Gets or sets the destination that should be ensured as the base layer before displaying a popup when
        /// <see cref="PopupStrategy"/> is <see cref="AnchorPopupStrategy.EnsureBaseAndPopup"/>.
        /// </summary>
        public string PopupBaseDestination
        {
            get => this.popupBaseDestination;
            set => this.popupBaseDestination = value;
        }

        /// <summary>
        /// Gets or sets the arguments used when navigating to <see cref="PopupBaseDestination"/>.
        /// </summary>
        public IList<Argument> PopupBaseArguments
        {
            get => this.popupBaseArguments;
            set => this.popupBaseArguments = new List<Argument>(value);
        }

        /// <summary>
        /// Creates a deep copy of this options instance.
        /// </summary>
        public AnchorNavOptions Clone()
        {
            return new AnchorNavOptions
            {
                stackStrategy = this.stackStrategy,
                popUpToDestination = this.popUpToDestination,
                popUpToInclusive = this.popUpToInclusive,
                enterAnim = this.enterAnim,
                exitAnim = this.exitAnim,
                popEnterAnim = this.popEnterAnim,
                popExitAnim = this.popExitAnim,
                popUpToSaveState = this.popUpToSaveState,
                restoreState = this.restoreState,
                launchSingleTop = this.launchSingleTop,
                popupStrategy = this.popupStrategy,
                popupBaseDestination = this.popupBaseDestination,
                popupBaseArguments = this.popupBaseArguments != null
                    ? new List<Argument>(this.popupBaseArguments)
                    : new List<Argument>(),
            };
        }
    }
}
