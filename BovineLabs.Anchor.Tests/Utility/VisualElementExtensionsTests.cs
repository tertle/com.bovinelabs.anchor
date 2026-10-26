namespace BovineLabs.Anchor.Tests.Utility
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using NUnit.Framework;
    using Unity.AppUI.UI;
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

        [Test]
        public void ReleaseResourcesRecursive_ClearsPressableCommandsBeforeRelease()
        {
            var command = new RelayCommand(static () => { });
            var root = new VisualElement();
            var button = new ActionButton();
            button.clickable.command = command;
            root.Add(button);

            root.ReleaseResourcesRecursive();

            Assert.IsTrue(root.resourcesReleased);
            Assert.IsTrue(button.resourcesReleased);
            Assert.IsNull(button.clickable.command);
            Assert.DoesNotThrow(command.NotifyCanExecuteChanged);
        }

        private sealed class TestData
        {
            public string Text { get; set; }

            public int Number { get; set; }
        }
    }
}
