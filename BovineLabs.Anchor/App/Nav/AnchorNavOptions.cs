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

        /// <summary> The back stack will be cleared before navigating. </summary>
        PopAll,
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
        [Tooltip("Back stack manipulation strategy executed before navigating.")]
        private AnchorStackStrategy stackStrategy;

        [SerializeField]
        [Tooltip("Route identifier to pop up to before navigating.")]
        private string popUpToDestination;

        [SerializeField]
        [Tooltip("Reuse the existing top destination instead of pushing a duplicate.")]
        private bool launchSingleTop = true;

        [SerializeField]
        [Tooltip("Presentation strategy applied when treating the navigation as a popup.")]
        private AnchorPopupStrategy popupStrategy;

        [SerializeField]
        [Tooltip("Destination ensured as the base before displaying the popup.")]
        private string popupBaseDestination;

        [SerializeField]
        [Tooltip("Arguments supplied when navigating to the popup base destination.")]
        private List<Argument> popupBaseArguments = new();

        [SerializeField]
        [Tooltip("Animation used when presenting this destination.")]
        private NavigationAnimation enterAnim;

        [SerializeField]
        [Tooltip("Animation used when leaving this destination.")]
        private NavigationAnimation exitAnim;

        [SerializeField]
        [Tooltip("Animation played when this destination returns via a pop.")]
        private NavigationAnimation popEnterAnim;

        [SerializeField]
        [Tooltip("Animation played when this destination is popped off the stack.")]
        private NavigationAnimation popExitAnim;

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
        /// <returns> A cloned copy. </returns>
        public AnchorNavOptions Clone()
        {
            return new AnchorNavOptions
            {
                stackStrategy = this.stackStrategy,
                popUpToDestination = this.popUpToDestination,
                enterAnim = this.enterAnim,
                exitAnim = this.exitAnim,
                popEnterAnim = this.popEnterAnim,
                popExitAnim = this.popExitAnim,
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
