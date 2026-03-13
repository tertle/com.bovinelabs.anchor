// <copyright file="AnimationDescription.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// An animation description. It contains the duration of the animation, the easing function and the callback.
    /// </summary>
    public struct AnimationDescription
    {
        public static readonly AnimationDescription None = new()
        {
            Easing = UnityEngine.UIElements.Experimental.Easing.Linear,
            DurationMs = 0,
            Callback = null,
        };

        /// <summary>
        /// The duration of the animation in milliseconds.
        /// </summary>
        public int DurationMs;

        /// <summary>
        /// The easing function to use for the animation.
        /// </summary>
        public Func<float, float> Easing;

        /// <summary>
        /// The callback to call when the animation is running.
        /// </summary>
        public Action<VisualElement, float> Callback;
    }
}
