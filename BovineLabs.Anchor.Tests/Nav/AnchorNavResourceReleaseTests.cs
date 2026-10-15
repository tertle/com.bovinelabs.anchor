namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorNavResourceReleaseTests
    {
        [Test]
        public void Navigate_ReleasesDiscardedScreenAfterExitCallbacks()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            var screen = new TestContentContainerElement();
            var receiver = new TestNavigationScreenReceiver();
            screen.Leaf.dataSource = receiver;
            scope.ServiceProvider.GetRequiredService<TestVisualElementFactory>().Register("first", () => screen);
            var host = new AnchorNavHost();
            scope.App.RootVisualElement.Add(host);
            scope.App.NavHost = host;
            var exitCalls = 0;
            host.ExitedDestination += (_, element, _) =>
            {
                exitCalls++;
                Assert.IsFalse(element.resourcesReleased);
                Assert.IsFalse(screen.Decoration.resourcesReleased);
                Assert.IsNotNull(element.parent);
                Assert.AreEqual(1, receiver.ExitCount);
            };

            host.Navigate("first");
            Assert.DoesNotThrow(() => host.Navigate("second"));

            Assert.AreEqual(1, exitCalls);
            Assert.IsNull(screen.parent);
            Assert.IsFalse(screen.resourcesReleased);
            host.ReleaseVisualGeneration();
            Assert.IsTrue(screen.resourcesReleased);
            Assert.IsTrue(screen.Content.resourcesReleased);
            Assert.IsTrue(screen.Decoration.resourcesReleased);
            Assert.IsTrue(screen.Leaf.resourcesReleased);
            Assert.AreEqual("second", host.CurrentDestination);
        }

        [Test]
        public void ClosePopup_ReleasesPopupAndPreservesBaseScreen()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");
            harness.Host.Navigate("base");
            var baseScreen = harness.Host[0];
            harness.Host.Navigate("popup", new AnchorNavOptions {
                PopupStrategy = AnchorPopupStrategy.PopupOnCurrent
            });
            var popup = harness.Host[1];
            var popupChild = popup[0];

            Assert.IsTrue(harness.Host.ClosePopup("popup"));

            Assert.IsFalse(popup.resourcesReleased);
            harness.Host.ReleaseVisualGeneration();
            Assert.IsTrue(popup.resourcesReleased);
            Assert.IsTrue(popupChild.resourcesReleased);
            Assert.IsFalse(baseScreen.resourcesReleased);
            Assert.AreSame(baseScreen, harness.Host[0]);
            Assert.AreEqual("base", harness.Host.CurrentDestination);
            harness.Host.ClearNavigation();
        }

        [Test]
        public void ButtonCommand_NavigatesBeforeDiscardingItsVisualResources()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            var host = new AnchorNavHost();
            scope.App.RootVisualElement.Add(host);
            scope.App.NavHost = host;
            var screen = new VisualElement();
            var button = new AnchorButton();
            screen.Add(button);
            button.commandWithEventInfo = new RelayCommand<EventBase>(_ =>
            {
                host.Navigate("next");
                Assert.IsFalse(button.resourcesReleased);
                Assert.IsNull(screen.parent);
            });
            scope.ServiceProvider.GetRequiredService<TestVisualElementFactory>().Register("first", () => screen);
            host.Navigate("first");

            Assert.DoesNotThrow(() => button.clickable.InvokePressed(null));

            Assert.AreEqual("next", host.CurrentDestination);
            Assert.IsFalse(button.resourcesReleased);
            host.ReleaseVisualGeneration();
            Assert.IsTrue(screen.resourcesReleased);
            Assert.IsTrue(button.resourcesReleased);
        }

        [Test]
        public void Navigate_SameDestinationPreservesReusableScreen()
        {
            using var harness = new TestAnchorNavHostHarness();
            var receiver = harness.RegisterScreen("screen");
            harness.Host.Navigate("screen", AnchorNavArgument.String("value", "1"));
            var screen = harness.Host[0];

            harness.Host.Navigate("screen", AnchorNavArgument.String("value", "2"));

            Assert.AreSame(screen, harness.Host[0]);
            Assert.IsFalse(screen.resourcesReleased);
            Assert.AreEqual(2, receiver.EnterCount);
            Assert.AreEqual(0, receiver.ExitCount);
            harness.Host.ClearNavigation();
        }

        [Test]
        public void CompleteImmediately_AfterExternalReleaseSkipsVisualCallbackAndCompletesOnce()
        {
            var element = new VisualElement();
            var visualCallbacks = 0;
            var completions = 0;
            var handle = new AnchorNavAnimationHandle(
                element,
                new AnimationDescription {
                    Callback = (_, _) => visualCallbacks++
                },
                () => completions++);
            element.ReleaseResources();

            handle.CompleteImmediately();
            handle.CompleteImmediately();

            Assert.AreEqual(0, visualCallbacks);
            Assert.AreEqual(1, completions);
            Assert.IsFalse(handle.TryFinalizeFromAnimation());
        }
    }
}
