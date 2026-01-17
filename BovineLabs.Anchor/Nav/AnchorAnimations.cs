// <copyright file="AnchorAnimations.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Configurable navigation animation set applied when entering and exiting destinations.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the animation used when navigating to the destination.
        /// </summary>
        public AnchorNavAnimation EnterAnim
        {
            get => this.enterAnimation;
            set => this.enterAnimation = value;
        }

        /// <summary>
        /// Gets or sets the animation used when navigating away from the current destination.
        /// </summary>
        public AnchorNavAnimation ExitAnim
        {
            get => this.exitAnimation;
            set => this.exitAnimation = value;
        }

        /// <summary>
        /// Gets or sets the animation that plays when this destination re-enters the stack after a pop operation.
        /// </summary>
        public AnchorNavAnimation PopEnterAnim
        {
            get => this.popEnterAnimation;
            set => this.popEnterAnimation = value;
        }

        /// <summary>
        /// Gets or sets the animation that plays when this destination is popped from the stack.
        /// </summary>
        public AnchorNavAnimation PopExitAnim
        {
            get => this.popExitAnimation;
            set => this.popExitAnimation = value;
        }

        /// <summary>
        /// Creates a deep copy of this animation instance.
        /// </summary>
        /// <returns> A cloned copy. </returns>
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
