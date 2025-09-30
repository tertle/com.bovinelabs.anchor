// <copyright file="AnchorAnimations.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.AppUI.Navigation;
    using UnityEngine;

    [Serializable]
    public class AnchorAnimations
    {
        [SerializeField]
        [Tooltip("Animation used when presenting this destination.")]
        private NavigationAnimation enterAnim = NavigationAnimation.None;

        [SerializeField]
        [Tooltip("Animation used when leaving this destination.")]
        private NavigationAnimation exitAnim = NavigationAnimation.None;

        [SerializeField]
        [Tooltip("Animation played when this destination returns via a pop.")]
        private NavigationAnimation popEnterAnim = NavigationAnimation.None;

        [SerializeField]
        [Tooltip("Animation played when this destination is popped off the stack.")]
        private NavigationAnimation popExitAnim = NavigationAnimation.None;

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
        /// Creates a deep copy of this animation instance.
        /// </summary>
        /// <returns> A cloned copy. </returns>
        public AnchorAnimations Clone()
        {
            return new AnchorAnimations()
            {
                enterAnim = this.enterAnim,
                exitAnim = this.exitAnim,
                popEnterAnim = this.popEnterAnim,
                popExitAnim = this.popExitAnim,
            };
        }
    }
}
