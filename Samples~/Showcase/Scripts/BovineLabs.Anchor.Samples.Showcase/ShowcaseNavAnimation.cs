namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Asset;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    // Unsaved app-owned instances demonstrate custom transitions without registering sample assets in AnchorSettings.
    public sealed class ShowcaseNavAnimation : AnchorNavAnimation
    {
        private bool _entering;
        private bool _scale;

        protected override Func<float, float> EasingFunction => Easing.OutCubic;

        protected override Action<VisualElement, float> Callback => Animate;

        protected override int DefaultDuration => 500;

        public void Configure(int id, bool entering, bool scale)
        {
            ((IUID)this).ID = id;
            _entering = entering;
            _scale = scale;
            Reset();
        }

        private void Animate(VisualElement element, float progress)
        {
            element.style.opacity = _entering ? progress : 1 - progress;
            if (_scale)
            {
                var scale = _entering ? Mathf.Lerp(1.12f, 1, progress) : Mathf.Lerp(1, 0.92f, progress);
                element.style.scale = new Scale(new Vector3(scale, scale, 1));
            }
        }
    }
}
