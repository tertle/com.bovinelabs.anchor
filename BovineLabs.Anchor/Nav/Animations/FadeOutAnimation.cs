// <copyright file="FadeOutAnimation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav.Animations
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    /// <summary> Fade out animation. </summary>
    public class FadeOutAnimation : AnchorNavAnimation
    {
        /// <inheritdoc/>
        protected override Func<float, float> EasingFunction { get; } = Easing.OutCubic;

        /// <inheritdoc/>
        protected override Action<VisualElement, float> Callback { get; } = (v, f) => v.style.opacity = 1.0f - f;
    }
}
