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
        private AnchorStackStrategy _stackStrategy;

        [SerializeField]
        [Tooltip("Destination key used by PopToSpecificDestination.")]
        private string _popupToDestination;

        [SerializeField]
        [Tooltip("Presentation strategy applied when treating the navigation as a popup.")]
        private AnchorPopupStrategy _popupStrategy;

        [SerializeField]
        [Tooltip("Destination ensured as the base before displaying the popup.")]
        private string _popupBaseDestination;

        [SerializeField]
        [Tooltip("Arguments supplied when navigating to the popup base destination.")]
        private List<AnchorNavArgument> _popupBaseArguments = new();

        [SerializeField]
        [Tooltip("How to treat any existing popups when this navigation is shown as a popup.")]
        private AnchorPopupExistingStrategy _popupExistingStrategy;

        [SerializeField]
        private AnchorAnimations _animations = new();

        public AnchorStackStrategy StackStrategy
        {
            get => _stackStrategy;
            set => _stackStrategy = value;
        }

        public string PopupToDestination
        {
            get => _popupToDestination;
            set => _popupToDestination = value;
        }

        public AnchorPopupStrategy PopupStrategy
        {
            get => _popupStrategy;
            set => _popupStrategy = value;
        }

        public AnchorPopupExistingStrategy PopupExistingStrategy
        {
            get => _popupExistingStrategy;
            set => _popupExistingStrategy = value;
        }

        public string PopupBaseDestination
        {
            get => _popupBaseDestination;
            set => _popupBaseDestination = value;
        }

        public IList<AnchorNavArgument> PopupBaseArguments
        {
            get => _popupBaseArguments;
            set => _popupBaseArguments = value != null ? new List<AnchorNavArgument>(value) : new List<AnchorNavArgument>();
        }

        public AnchorAnimations Animations
        {
            get => _animations;
            set => _animations = value ?? new AnchorAnimations();
        }

        public AnchorNavOptions Clone()
        {
            return new AnchorNavOptions
            {
                _stackStrategy = _stackStrategy,
                _popupToDestination = _popupToDestination,
                _animations = _animations.Clone(),
                _popupStrategy = _popupStrategy,
                _popupExistingStrategy = _popupExistingStrategy,
                _popupBaseDestination = _popupBaseDestination,
                _popupBaseArguments = _popupBaseArguments != null ? new List<AnchorNavArgument>(_popupBaseArguments) : new List<AnchorNavArgument>(),
            };
        }
    }
}
