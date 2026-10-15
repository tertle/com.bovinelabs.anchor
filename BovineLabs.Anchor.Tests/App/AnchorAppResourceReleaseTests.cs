namespace BovineLabs.Anchor.Tests.App
{
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Anchor.Toolbar;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorAppResourceReleaseTests
    {
        [Test]
        public void ReleaseVisualGeneration_ReleasesPhysicalTreeAfterToolbarCleanupAndRetainsServices()
        {
            var toolbar = new RecordingToolbarHost();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(IAnchorToolbarHost), toolbar);
            using var provider = services.BuildServiceProvider();
            using var app = new AnchorApp();
            var panel = new TestPanel();
            app.Initialize(provider);
            app.SetPanel(panel);
            app.Theme = "custom";
            app.Scale = "large";
            app.InitializeToolbar();
            var toolbarChild = toolbar.Root[0];

            app.ReleaseVisualGeneration();

            Assert.IsTrue(toolbar.ReleasedWhileLive);
            Assert.IsTrue(toolbar.Root.resourcesReleased);
            Assert.IsTrue(toolbarChild.resourcesReleased);
            Assert.IsTrue(panel.Root.resourcesReleased);
            Assert.IsTrue(panel.Root.Content.resourcesReleased);
            Assert.IsTrue(panel.Root.Decoration.resourcesReleased);
            Assert.IsTrue(panel.Root.Leaf.resourcesReleased);
            Assert.AreSame(provider, app.Services);
            Assert.AreSame(toolbar, app.Services.GetRequiredService<IAnchorToolbarHost>());
            Assert.IsNull(app.RootVisualElement);
            Assert.IsNull(app.Panel);

            var replacement = new TestPanel();
            app.SetPanel(replacement);
            Assert.AreEqual("custom", replacement.Theme);
            Assert.AreEqual("large", replacement.Scale);
            Assert.IsFalse(replacement.Root.resourcesReleased);
        }

        [Test]
        public void ReleaseVisualGeneration_AfterPanelRendererReleasesTreeIsSafe()
        {
            using var scope = new TestAnchorAppScope();
            var root = scope.App.RootVisualElement;
            root.hierarchy.Clear(VisualElementClearOptions.RecursiveReleaseResources);
            root.ReleaseResources();

            Assert.DoesNotThrow(() => scope.App.ReleaseVisualGeneration());
            Assert.DoesNotThrow(() => scope.App.ReleaseVisualGeneration());

            Assert.IsNull(scope.App.RootVisualElement);
            Assert.IsNull(scope.App.NavHost);
            Assert.AreSame(scope.ServiceProvider, scope.App.Services);
        }

        [Test]
        public void ReleaseVisualGeneration_PreservesCapturedNavigationAndStateHandlesForNewVisuals()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            var host = new AnchorNavHost();
            scope.App.RootVisualElement.Add(host);
            scope.App.NavHost = host;
            host.Navigate("first");
            var firstScreen = host[0];
            var stateHandle = host.SaveStateHandle();
            host.Navigate("second");
            var secondScreen = host[0];
            var state = host.CaptureReloadState();

            scope.App.ReleaseVisualGeneration();

            Assert.IsTrue(firstScreen.resourcesReleased);
            Assert.IsTrue(secondScreen.resourcesReleased);
            Assert.IsTrue(host.resourcesReleased);
            scope.App.SetPanel(new TestPanel());
            var replacement = new AnchorNavHost();
            scope.App.RootVisualElement.Add(replacement);
            scope.App.NavHost = replacement;
            replacement.RestoreReloadState(state);

            Assert.AreEqual("second", replacement.CurrentDestination);
            Assert.IsTrue(replacement.CanGoBack);
            Assert.IsFalse(replacement[0].resourcesReleased);
            Assert.IsTrue(replacement.ReleaseStateHandle(stateHandle));
            Assert.AreEqual("first", replacement.CurrentDestination);
            Assert.AreNotSame(firstScreen, replacement[0]);
        }

        private sealed class TestPanel : IAnchorPanel
        {
            public TestContentContainerElement Root { get; } = new();

            public VisualElement RootVisualElement => Root;

            public string Theme { get; set; } = "dark";

            public string Scale { get; set; } = "medium";
        }

        private sealed class RecordingToolbarHost : IAnchorToolbarHost
        {
            public VisualElement Root { get; } = new();

            public bool ReleasedWhileLive { get; private set; }

            public VisualElement CreateRootVisualElement()
            {
                Root.Add(new VisualElement());
                return Root;
            }

            public void ReleaseRootVisualElement()
            {
                ReleasedWhileLive = !Root.resourcesReleased;
            }
        }
    }
}
