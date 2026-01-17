// <copyright file="ScaleDownFadeInAnimation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav.Animations
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    /// <summary> Scale down and fade in animation. </summary>
    public class ScaleDownFadeInAnimation : AnchorNavAnimation
    {
        [SerializeField]
        private float startScale = 1.2f;

        [SerializeField]
        private float scaleDelta = 0.2f;

        protected override Func<float, float> EasingFunction { get; } = Easing.OutCubic;

        protected override Action<VisualElement, float> Callback => this.Function;

        private void Function(VisualElement v, float f)
        {
            var delta = this.startScale - (f * this.scaleDelta);
            v.style.scale = new Scale(new Vector3(delta, delta, 1));
            v.style.opacity = f;
        }
    }
}
