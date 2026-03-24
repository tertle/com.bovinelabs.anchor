// <copyright file="AnchorSafeAreaTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Elements
{
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorSafeAreaTests
    {
        [Test]
        public void Constructor_DefaultsToAllEdges_AndUsesSelfAsContentContainer()
        {
            var safeArea = new AnchorSafeArea();
            var child = new VisualElement();

            safeArea.Add(child);

            Assert.AreEqual(AnchorSafeAreaEdges.All, safeArea.Edges);
            Assert.AreSame(safeArea, safeArea.contentContainer);
            Assert.AreSame(child, safeArea.contentContainer[0]);
            Assert.AreEqual(1, safeArea.childCount);
        }

        [Test]
        public void TryCalculatePadding_FullScreen_ReturnsConvertedInsets()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 0f, 200f, 100f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.All,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_BelowUnsafeTopBand_SkipsTopPadding()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 8f, 200f, 92f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_WhenAtTop_AppliesTopPadding()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 0f, 200f, 92f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_ToolbarVisibleContentBelowToolbar_DoesNotAddTopInset()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 12f, 200f, 88f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_ToolbarHiddenContentAtTop_AddsTopInset()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 0f, 200f, 88f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_PartialOverlap_ClampsToIntersectingUnsafeArea()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(100f, 50f, 200f, 100f),
                new Rect(105f, 52f, 40f, 20f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.All,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_RespectsEdgeFlags()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 200f, 100f),
                new Rect(0f, 0f, 200f, 100f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right,
                out var padding);

            Assert.IsTrue(success);
            Assert.That(padding.Left, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Top, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(padding.Right, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(padding.Bottom, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryCalculatePadding_InvalidGeometry_ReturnsFalse()
        {
            var success = AnchorSafeAreaUtility.TryCalculatePadding(
                new Rect(0f, 0f, 0f, 100f),
                new Rect(0f, 0f, 200f, 100f),
                new Rect(50f, 20f, 900f, 460f),
                new Vector2(1000f, 500f),
                AnchorSafeAreaEdges.All,
                out _);

            Assert.IsFalse(success);
        }
    }
}
