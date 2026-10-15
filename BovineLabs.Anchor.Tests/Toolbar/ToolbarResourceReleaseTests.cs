namespace BovineLabs.Anchor.Tests.Toolbar
{
    using System;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine.UIElements;
    using ToolbarService = BovineLabs.Anchor.Debug.Toolbar.Toolbar;

    public class ToolbarResourceReleaseTests
    {
        [Test]
        public void ReleaseRootVisualElement_ReleasesHiddenGroupsAndDetachedScrollersAfterCustomDisposal()
        {
            var storage = new TestLocalStorageService();
            storage.SetValue("bl.show-ribbon", true);
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new EmptyServiceProvider(), viewModel, storage, Array.Empty<Type>());
            var root = (ToolbarView)toolbar.CreateRootVisualElement();
            var first = new RecordingElement();
            var second = new RecordingElement();
            root.AddRegistration(1, "First", "First", first);
            root.AddRegistration(2, "Second", "Second", second);
            var firstContainer = first.parent;
            var secondContainer = second.parent;
            var firstGroup = first.GetFirstAncestorOfType<ToolbarGroupElement>();
            var secondGroup = second.GetFirstAncestorOfType<ToolbarGroupElement>();
            var firstScroller = firstGroup.horizontalScroller;
            var secondScroller = secondGroup.horizontalScroller;
            root.ShowRibbon(false);

            toolbar.ReleaseRootVisualElement();

            Assert.IsTrue(first.DisposedWhileLive);
            Assert.IsTrue(second.DisposedWhileLive);
            Assert.IsTrue(first.resourcesReleased);
            Assert.IsTrue(second.resourcesReleased);
            Assert.IsTrue(first.Child.resourcesReleased);
            Assert.IsTrue(second.Child.resourcesReleased);
            Assert.IsTrue(firstContainer.resourcesReleased);
            Assert.IsTrue(secondContainer.resourcesReleased);
            Assert.IsTrue(firstGroup.resourcesReleased);
            Assert.IsTrue(secondGroup.resourcesReleased);
            Assert.IsTrue(firstScroller.resourcesReleased);
            Assert.IsTrue(secondScroller.resourcesReleased);
            Assert.IsTrue(root.resourcesReleased);
        }

        [Test]
        public void RemoveRegistration_ReleasesRemovedContainerAndKeepsTabReusable()
        {
            var storage = new TestLocalStorageService();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new EmptyServiceProvider(), viewModel, storage, Array.Empty<Type>());
            var root = (ToolbarView)toolbar.CreateRootVisualElement();
            var element = new RecordingElement();
            root.AddRegistration(1, "Tab", "First", element);
            var container = element.parent;
            var group = element.GetFirstAncestorOfType<ToolbarGroupElement>();

            root.RemoveRegistration(1);

            Assert.IsTrue(element.DisposedWhileLive);
            Assert.IsFalse(element.resourcesReleased);
            Assert.IsFalse(container.resourcesReleased);
            Assert.IsFalse(group.resourcesReleased);
            var replacement = new RecordingElement();
            root.AddRegistration(2, "Tab", "Second", replacement);
            Assert.AreSame(group, replacement.GetFirstAncestorOfType<ToolbarGroupElement>());
            Assert.IsFalse(replacement.resourcesReleased);
            toolbar.ReleaseRootVisualElement();
            Assert.IsTrue(element.resourcesReleased);
            Assert.IsTrue(container.resourcesReleased);
        }

        [Test]
        public void ButtonCommand_RemovesItsRegistrationBeforeDiscardingVisualResources()
        {
            var storage = new TestLocalStorageService();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new EmptyServiceProvider(), viewModel, storage, Array.Empty<Type>());
            var root = (ToolbarView)toolbar.CreateRootVisualElement();
            var button = new AnchorButton();
            root.AddRegistration(1, "Tab", "Button", button);
            var container = button.parent;
            button.commandWithEventInfo = new RelayCommand<EventBase>(_ => root.RemoveRegistration(1));

            Assert.DoesNotThrow(() => button.clickable.InvokePressed(null));

            Assert.IsNull(button.parent);
            Assert.IsNull(container.parent);
            Assert.IsFalse(button.resourcesReleased);
            toolbar.ReleaseRootVisualElement();
            Assert.IsTrue(button.resourcesReleased);
            Assert.IsTrue(container.resourcesReleased);
        }

        private sealed class RecordingElement : VisualElement, IDisposable
        {
            public RecordingElement()
            {
                Add(Child);
            }

            public VisualElement Child { get; } = new();

            public bool DisposedWhileLive { get; private set; }

            public void Dispose()
            {
                DisposedWhileLive = !resourcesReleased && !Child.resourcesReleased;
            }
        }

        private sealed class EmptyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return null;
            }
        }
    }
}
