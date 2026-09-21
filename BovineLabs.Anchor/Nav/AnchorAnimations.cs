namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    [Serializable]
    public class AnchorAnimations
    {
        [SerializeField]
        [Tooltip("Animation used when presenting this destination.")]
        private AnchorNavAnimation _enterAnimation;

        [SerializeField]
        [Tooltip("Animation used when leaving this destination.")]
        private AnchorNavAnimation _exitAnimation;

        [SerializeField]
        [Tooltip("Animation played when this destination returns via a pop.")]
        private AnchorNavAnimation _popEnterAnimation;

        [SerializeField]
        [Tooltip("Animation played when this destination is popped off the stack.")]
        private AnchorNavAnimation _popExitAnimation;

        public AnchorNavAnimation EnterAnim
        {
            get => _enterAnimation;
            set => _enterAnimation = value;
        }

        public AnchorNavAnimation ExitAnim
        {
            get => _exitAnimation;
            set => _exitAnimation = value;
        }

        public AnchorNavAnimation PopEnterAnim
        {
            get => _popEnterAnimation;
            set => _popEnterAnimation = value;
        }

        public AnchorNavAnimation PopExitAnim
        {
            get => _popExitAnimation;
            set => _popExitAnimation = value;
        }

        public AnchorAnimations Clone()
        {
            return new AnchorAnimations()
            {
                _enterAnimation = _enterAnimation,
                _exitAnimation = _exitAnimation,
                _popEnterAnimation = _popEnterAnimation,
                _popExitAnimation = _popExitAnimation,
            };
        }
    }
}
