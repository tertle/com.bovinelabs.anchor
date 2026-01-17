// <copyright file="AnchorNavAnimation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.ObjectManagement;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for Anchor navigation animations.
    /// </summary>
    [AutoRef("AnchorSettings", "animations", nameof(AnchorNavAnimation), "UI/Animations")]
    public abstract class AnchorNavAnimation : ScriptableObject
    {
        [Min(0)]
        [Tooltip("Animation time in milliseconds")]
        public int Duration;

        /// <summary>
        /// Gets the easing function to use for the animation.
        /// </summary>
        protected abstract Func<float, float> EasingFunction { get; }

        /// <summary> Gets the callback to call when the animation is running. </summary>
        protected abstract Action<VisualElement, float> Callback { get; }

        /// <summary>
        /// Gets the animation description for this animation.
        /// </summary>
        /// <returns>Animation description used by UI Toolkit animations.</returns>
        public AnimationDescription GetDescription()
        {
            return new AnimationDescription
            {
                easing = this.EasingFunction,
                durationMs = this.Duration,
                callback = this.Callback,
            };
        }
    }
}
