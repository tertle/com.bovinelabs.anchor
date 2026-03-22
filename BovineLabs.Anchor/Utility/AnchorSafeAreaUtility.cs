// <copyright file="AnchorSafeAreaUtility.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine;
    using UnityEngine.UIElements;

    internal static class AnchorSafeAreaUtility
    {
        internal readonly struct Padding
        {
            public Padding(float left, float top, float right, float bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }

            public float Left { get; }

            public float Top { get; }

            public float Right { get; }

            public float Bottom { get; }
        }

        internal static bool TryCalculatePadding(
            Rect panelRootWorldBound, Rect elementWorldBound, Rect safeArea, Vector2 screenSize, AnchorSafeAreaEdges edges, out Padding padding)
        {
            padding = default;

            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                return false;
            }

            if (panelRootWorldBound.width <= 0f || panelRootWorldBound.height <= 0f)
            {
                return false;
            }

            if (elementWorldBound.width <= 0f || elementWorldBound.height <= 0f)
            {
                return false;
            }

            var leftInset = (safeArea.x / screenSize.x) * panelRootWorldBound.width;
            var rightInset = ((screenSize.x - safeArea.xMax) / screenSize.x) * panelRootWorldBound.width;
            var topInset = ((screenSize.y - safeArea.yMax) / screenSize.y) * panelRootWorldBound.height;
            var bottomInset = (safeArea.y / screenSize.y) * panelRootWorldBound.height;

            var left = edges.HasFlag(AnchorSafeAreaEdges.Left)
                ? CalculateOverlap(elementWorldBound.xMin, elementWorldBound.xMax, panelRootWorldBound.xMin, panelRootWorldBound.xMin + leftInset)
                : 0f;

            var right = edges.HasFlag(AnchorSafeAreaEdges.Right)
                ? CalculateOverlap(elementWorldBound.xMin, elementWorldBound.xMax, panelRootWorldBound.xMax - rightInset, panelRootWorldBound.xMax)
                : 0f;

            var top = edges.HasFlag(AnchorSafeAreaEdges.Top)
                ? CalculateOverlap(elementWorldBound.yMin, elementWorldBound.yMax, panelRootWorldBound.yMin, panelRootWorldBound.yMin + topInset)
                : 0f;

            var bottom = edges.HasFlag(AnchorSafeAreaEdges.Bottom)
                ? CalculateOverlap(elementWorldBound.yMin, elementWorldBound.yMax, panelRootWorldBound.yMax - bottomInset, panelRootWorldBound.yMax)
                : 0f;

            padding = new Padding(left, top, right, bottom);
            return true;
        }

        internal static void ApplyPadding(VisualElement measurementElement, VisualElement targetElement, VisualElement panelRoot, AnchorSafeAreaEdges edges)
        {
            if (measurementElement == null || targetElement == null || panelRoot == null)
            {
                ResetPadding(targetElement?.style);
                return;
            }

            if (!TryCalculatePadding(panelRoot.worldBound, measurementElement.worldBound, AnchorApp.SafeArea, new Vector2(Screen.width, Screen.height), edges,
                out var padding))
            {
                ResetPadding(targetElement.style);
                return;
            }

            ApplyPadding(targetElement.style, padding);
        }

        internal static void ResetPadding(IStyle style)
        {
            if (style == null)
            {
                return;
            }

            style.paddingLeft = 0f;
            style.paddingTop = 0f;
            style.paddingRight = 0f;
            style.paddingBottom = 0f;
        }

        private static void ApplyPadding(IStyle style, Padding padding)
        {
            style.paddingLeft = padding.Left;
            style.paddingTop = padding.Top;
            style.paddingRight = padding.Right;
            style.paddingBottom = padding.Bottom;
        }

        private static float CalculateOverlap(float elementMin, float elementMax, float unsafeMin, float unsafeMax)
        {
            return Mathf.Max(0f, Mathf.Min(elementMax, unsafeMax) - Mathf.Max(elementMin, unsafeMin));
        }
    }
}
