// <copyright file="FadeInAnimation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav.Animations
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    /// <summary> Fade in animation. </summary>
    public class FadeInAnimation : AnchorNavAnimation
    {
        protected override Func<float, float> EasingFunction { get; } = Easing.OutCubic;

        protected override Action<VisualElement, float> Callback { get; } = (v, f) => v.style.opacity = f;
    }
}
