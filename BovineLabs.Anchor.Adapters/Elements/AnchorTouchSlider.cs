// <copyright file="AnchorTouchSlider.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shared Anchor behavior applied to App UI touch sliders.
    /// </summary>
    internal static class AnchorTouchSlider
    {
        /// <summary>
        /// Additional class used to scope the overflow workaround override.
        /// </summary>
        public const string WorkaroundUssClassName = "bl-touchslider-workaround";

        public static readonly BindingId SizeProperty = nameof(TouchSliderFloat.size);
        public static readonly BindingId LabelProperty = nameof(TouchSliderFloat.label);

        /// <summary>
        /// Adds Anchor's focused-editing behavior to an App UI touch slider.
        /// </summary>
        /// <param name="slider">The slider to initialize.</param>
        public static void Initialize(VisualElement slider)
        {
            slider.AddToClassList(WorkaroundUssClassName);
            slider.Q<UnityEngine.UIElements.TextField>(TouchSlider<float>.valueUssClassName).RegisterCallback<FocusEvent>(OnInputFocused);
        }

        /// <summary>
        /// Keeps App UI's progress fill inside the border while allowing the input editor to overflow.
        /// </summary>
        /// <param name="slider">The slider element.</param>
        /// <param name="progress">The App UI progress element.</param>
        /// <param name="orientation">The slider orientation.</param>
        /// <param name="layoutDirection">The active layout direction.</param>
        public static void RefreshProgress(VisualElement slider, VisualElement progress, Direction orientation, Dir layoutDirection)
        {
            if (slider.panel == null || !slider.layout.IsValid())
            {
                return;
            }

            var leftInset = slider.resolvedStyle.borderLeftWidth;
            var rightInset = slider.resolvedStyle.borderRightWidth;
            var topInset = slider.resolvedStyle.borderTopWidth;
            var bottomInset = slider.resolvedStyle.borderBottomWidth;

            if (orientation == Direction.Horizontal)
            {
                var width = Mathf.Max(0, slider.layout.width - leftInset - rightInset) * GetNormalizedProgress(slider);
                var left = layoutDirection == Dir.Ltr ? leftInset : slider.layout.width - rightInset - width;

                progress.style.left = left;
                progress.style.right = StyleKeyword.Null;
                progress.style.width = width;
                progress.style.top = topInset;
                progress.style.bottom = bottomInset;
                progress.style.height = StyleKeyword.Null;
                return;
            }

            var height = Mathf.Max(0, slider.layout.height - topInset - bottomInset) * GetNormalizedProgress(slider);
            progress.style.top = slider.layout.height - bottomInset - height;
            progress.style.height = height;
            progress.style.left = leftInset;
            progress.style.right = rightInset;
            progress.style.bottom = StyleKeyword.Null;
            progress.style.width = StyleKeyword.Null;
        }

        private static float GetNormalizedProgress(VisualElement slider)
        {
            return slider switch
            {
                TouchSliderFloat floatSlider => Mathf.InverseLerp(floatSlider.lowValue, floatSlider.highValue, floatSlider.value),
                TouchSliderInt intSlider => Mathf.InverseLerp(intSlider.lowValue, intSlider.highValue, intSlider.value),
                _ => throw new System.ArgumentOutOfRangeException(nameof(slider), slider.GetType(), "Unsupported touch slider type."),
            };
        }

        private static void OnInputFocused(FocusEvent evt)
        {
            ((UnityEngine.UIElements.TextField)evt.currentTarget).SelectAll();
        }
    }
}
