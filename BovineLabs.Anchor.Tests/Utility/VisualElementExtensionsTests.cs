namespace BovineLabs.Anchor.Tests.Utility
{
    using BovineLabs.Anchor;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class VisualElementExtensionsTests
    {
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
