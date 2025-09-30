// <copyright file="AnchorNavOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using Unity.AppUI.Navigation;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Pre-navigation behaviour describing how the back stack is adjusted before activating the destination.
    /// </summary>
    public enum AnchorStackStrategy
    {
        /// <summary>Leave the back stack untouched prior to navigation.</summary>
        None,

        /// <summary>Pop entries until the specified destination is on top, when present.</summary>
        PopToSpecificDestination,

        /// <summary>
        /// Pop entries until the first destination in the current graph remains.
        /// </summary>
        PopToRoot,

        /// <summary>Remove every entry from the back stack before navigating.</summary>
        PopAll,
    }

    /// <summary>
    /// Determines whether a navigation request should be handled as a popup overlay or a standard destination change.
    /// </summary>
    [Serializable]
    public enum AnchorPopupStrategy
    {
        /// <summary>Treat the navigation as a normal destination change with no popup handling.</summary>
        None,

        /// <summary>Overlay the destination on top of the current visual stack without altering the base.</summary>
        PopupOnCurrent,

        /// <summary>Ensure a specific base destination is active before overlaying the popup.</summary>
        EnsureBaseAndPopup,
    }

    /// <summary>
    /// Specifies how existing popups should be treated when presenting a new popup destination.
    /// </summary>
    [Serializable]
    public enum AnchorPopupExistingStrategy
    {
        /// <summary>Keep any existing popups and add the new popup on top of the stack.</summary>
        None,

        /// <summary>Remove all current popups before the new popup is shown while leaving the base destination intact.</summary>
        CloseOtherPopups,

        /// <summary>When popups are active, archive the current stack, rebuild the base, and present the new popup (otherwise behaves like <see cref="None"/>).</summary>
        PushNew,
    }

    /// <summary>
    /// AnchorNavOptions stores special options for navigate actions.
    /// </summary>
    [Serializable]
    public class AnchorNavOptions
    {
        [SerializeField]
        [Tooltip("Back stack manipulation strategy executed before navigating.")]
        private AnchorStackStrategy stackStrategy;

        [SerializeField]
        [FormerlySerializedAs("popUpToDestination")]
        [Tooltip("Route identifier to pop up to before navigating.")]
        private string popupToDestination;

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
        [Tooltip("How to treat any existing popups when this navigation is shown as a popup.")]
        private AnchorPopupExistingStrategy popupExistingStrategy;

        [SerializeField]
        private AnchorAnimations animations = new();

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
        public string PopupToDestination
        {
            get => this.popupToDestination;
            set => this.popupToDestination = value;
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
        /// Gets or sets the behaviour applied to popups that are already present when this popup is requested.
        /// </summary>
        public AnchorPopupExistingStrategy PopupExistingStrategy
        {
            get => this.popupExistingStrategy;
            set => this.popupExistingStrategy = value;
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
        /// Gets or sets the destination that should be ensured as the base layer before displaying a popup when
        /// <see cref="PopupStrategy"/> is <see cref="AnchorPopupStrategy.EnsureBaseAndPopup"/>.
        /// </summary>
        public AnchorAnimations Animations
        {
            get => this.animations;
            set => this.animations = value;
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
                popupToDestination = this.popupToDestination,
                animations = this.animations.Clone(),
                popupStrategy = this.popupStrategy,
                popupExistingStrategy = this.popupExistingStrategy,
                popupBaseDestination = this.popupBaseDestination,
                popupBaseArguments = this.popupBaseArguments != null ? new List<Argument>(this.popupBaseArguments) : new List<Argument>(),
            };
        }
    }
}
