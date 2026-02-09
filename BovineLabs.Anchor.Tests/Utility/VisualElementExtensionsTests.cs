// <copyright file="VisualElementExtensionsTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using BovineLabs.Anchor;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class VisualElementExtensionsTests
    {
        [Test]
        public void BindingHelpers_ApplyWithoutThrowing()
        {
            var label = new Label();

            Assert.DoesNotThrow(() => label.SetBindingTwoWay(nameof(Label.text), nameof(TestData.Text)));
            Assert.DoesNotThrow(() => label.SetBindingToUI(nameof(Label.text), nameof(TestData.Text)));
            Assert.DoesNotThrow(() => label.SetBindingFromUI(nameof(Label.text), nameof(TestData.Text)));

            Assert.DoesNotThrow(() =>
                label.SetBindingTwoWay<int, string>(
                    nameof(Label.text),
                    nameof(TestData.Number),
                    static (ref int value) => value.ToString(),
                    static (ref string value) => int.Parse(value)));

            Assert.DoesNotThrow(() =>
                label.SetBindingToUI<int, string>(
                    nameof(Label.text),
                    nameof(TestData.Number),
                    static (ref int value) => value.ToString()));

            Assert.DoesNotThrow(() =>
                label.SetBindingFromUI<int, string>(
                    nameof(Label.text),
                    nameof(TestData.Number),
                    static (ref int value) => value.ToString()));
        }

        [Test]
        public void SetPickingModeRecursive_UpdatesEntireSubtree()
        {
            var root = new VisualElement();
            var child = new VisualElement();
            var grandChild = new VisualElement();
            child.Add(grandChild);
            root.Add(child);

            root.SetPickingModeRecursive(PickingMode.Ignore);

            Assert.AreEqual(PickingMode.Ignore, root.pickingMode);
            Assert.AreEqual(PickingMode.Ignore, child.pickingMode);
            Assert.AreEqual(PickingMode.Ignore, grandChild.pickingMode);
        }

        [Test]
        public void TryResolveDataSource_ReturnsFalse_ForNullOrMismatchedSource()
        {
            Assert.IsFalse(BovineLabs.Anchor.VisualElementExtensions.TryResolveDataSource<TestData>(null, out _));

            var root = new VisualElement { dataSource = new object() };
            var child = new VisualElement();
            root.Add(child);

            Assert.IsFalse(child.TryResolveDataSource<TestData>(out _));
        }

        [Test]
        public void TryResolveDataSource_ReturnsTrue_ForHierarchicalMatch()
        {
            var source = new TestData { Text = "hello" };
            var root = new VisualElement { dataSource = source };
            var child = new VisualElement();
            root.Add(child);

            var resolved = child.TryResolveDataSource<TestData>(out var data);

            Assert.IsTrue(resolved);
            Assert.AreSame(source, data);
        }

        private sealed class TestData
        {
            public string Text { get; set; }

            public int Number { get; set; }
        }
    }
}
