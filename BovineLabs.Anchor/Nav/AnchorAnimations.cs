namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    [Serializable]
    public class AnchorAnimations
    {
        [SerializeField]
        [Tooltip("Animation used when presenting this destination.")]
        private AnchorNavAnimation enterAnimation;

        [SerializeField]
        [Tooltip("Animation used when leaving this destination.")]
        private AnchorNavAnimation exitAnimation;

        [SerializeField]
        [Tooltip("Animation played when this destination returns via a pop.")]
        private AnchorNavAnimation popEnterAnimation;

        [SerializeField]
        [Tooltip("Animation played when this destination is popped off the stack.")]
        private AnchorNavAnimation popExitAnimation;

        public AnchorNavAnimation EnterAnim
        {
            get => this.enterAnimation;
            set => this.enterAnimation = value;
        }

        public AnchorNavAnimation ExitAnim
        {
            get => this.exitAnimation;
            set => this.exitAnimation = value;
        }

        public AnchorNavAnimation PopEnterAnim
        {
            get => this.popEnterAnimation;
            set => this.popEnterAnimation = value;
        }

        public AnchorNavAnimation PopExitAnim
        {
            get => this.popExitAnimation;
            set => this.popExitAnimation = value;
        }

        public AnchorAnimations Clone()
        {
            return new AnchorAnimations()
            {
                enterAnimation = this.enterAnimation,
                exitAnimation = this.exitAnimation,
                popEnterAnimation = this.popEnterAnimation,
                popExitAnimation = this.popExitAnimation,
            };
        }
    }
}
