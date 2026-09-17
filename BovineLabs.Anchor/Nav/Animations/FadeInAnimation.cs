namespace BovineLabs.Anchor.Nav.Animations
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    public class FadeInAnimation : AnchorNavAnimation
    {
        protected override Func<float, float> EasingFunction { get; } = Easing.OutCubic;

        protected override Action<VisualElement, float> Callback { get; } = (v, f) => v.style.opacity = f;
    }
}
