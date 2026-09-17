namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;

    public enum AnchorStackStrategy
    {
        None,

        /// <summary>
        /// Clears the stack if the target destination is absent.
        /// </summary>
        PopToSpecificDestination,

        PopToRoot,

        PopAll,
    }

    [Serializable]
    public enum AnchorPopupStrategy
    {
        None,

        PopupOnCurrent,

        EnsureBaseAndPopup,
    }

    [Serializable]
    public enum AnchorPopupExistingStrategy
    {
        None,

        CloseOtherPopups,

        /// <summary>
        /// Archives the current stack, rebuilds the base, and presents the new popup regardless of existing popups.
        /// </summary>
        PushNew,
    }

    [Serializable]
    public class AnchorNavOptions
    {
        [SerializeField]
        [Tooltip("Back stack manipulation strategy executed before navigating.")]
        private AnchorStackStrategy stackStrategy;

        [SerializeField]
        [FormerlySerializedAs("popUpToDestination")]
        [Tooltip("Destination key used by PopToSpecificDestination.")]
        private string popupToDestination;

        [SerializeField]
        [Tooltip("Presentation strategy applied when treating the navigation as a popup.")]
        private AnchorPopupStrategy popupStrategy;

        [SerializeField]
        [Tooltip("Destination ensured as the base before displaying the popup.")]
        private string popupBaseDestination;

        [SerializeField]
        [Tooltip("Arguments supplied when navigating to the popup base destination.")]
        private List<AnchorNavArgument> popupBaseArguments = new();

        [SerializeField]
        [Tooltip("How to treat any existing popups when this navigation is shown as a popup.")]
        private AnchorPopupExistingStrategy popupExistingStrategy;

        [SerializeField]
        private AnchorAnimations animations = new();

        public AnchorStackStrategy StackStrategy
        {
            get => this.stackStrategy;
            set => this.stackStrategy = value;
        }

        public string PopupToDestination
        {
            get => this.popupToDestination;
            set => this.popupToDestination = value;
        }

        public AnchorPopupStrategy PopupStrategy
        {
            get => this.popupStrategy;
            set => this.popupStrategy = value;
        }

        public AnchorPopupExistingStrategy PopupExistingStrategy
        {
            get => this.popupExistingStrategy;
            set => this.popupExistingStrategy = value;
        }

        public string PopupBaseDestination
        {
            get => this.popupBaseDestination;
            set => this.popupBaseDestination = value;
        }

        public IList<AnchorNavArgument> PopupBaseArguments
        {
            get => this.popupBaseArguments;
            set => this.popupBaseArguments = value != null ? new List<AnchorNavArgument>(value) : new List<AnchorNavArgument>();
        }

        public AnchorAnimations Animations
        {
            get => this.animations;
            set => this.animations = value ?? new AnchorAnimations();
        }

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
                popupBaseArguments = this.popupBaseArguments != null ? new List<AnchorNavArgument>(this.popupBaseArguments) : new List<AnchorNavArgument>(),
            };
        }
    }
}
