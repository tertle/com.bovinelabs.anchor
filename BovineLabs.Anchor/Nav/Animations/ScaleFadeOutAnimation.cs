namespace BovineLabs.Anchor.Nav.Animations
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    public class ScaleFadeOutAnimation : AnchorNavAnimation
    {
        [SerializeField]
        private float _startScale = 1.0f;

        [SerializeField]
        private float _endScale = 1.2f;

        protected override Func<float, float> EasingFunction { get; } = Easing.OutCubic;

        protected override Action<VisualElement, float> Callback => Function;

        protected override int DefaultDuration => 500;

        private void Function(VisualElement v, float f)
        {
            var delta = Mathf.Lerp(_startScale, _endScale, f);
            v.style.scale = new Scale(new Vector3(delta, delta, 1));
            v.style.opacity = 1.0f - f;
        }
    }
}
