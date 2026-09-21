namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using UnityEngine.UIElements;

    internal sealed class TestAnchorNavHostHarness : IDisposable
    {
        private readonly TestAnchorAppScope _scope;

        public TestAnchorNavHostHarness()
        {
            _scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            Factory = _scope.ServiceProvider.GetService(typeof(TestVisualElementFactory)) as TestVisualElementFactory;
            if (Factory == null)
            {
                throw new InvalidOperationException("Failed to resolve TestVisualElementFactory.");
            }

            Host = new AnchorNavHost();
            _scope.App.NavHost = Host;
        }

        public AnchorNavHost Host { get; }

        private TestVisualElementFactory Factory { get; }

        public TestNavigationScreenReceiver RegisterScreen(string destination)
        {
            var receiver = new TestNavigationScreenReceiver();

            Factory.Register(destination, () =>
            {
                var container = new VisualElement { name = destination };
                container.Add(new VisualElement { dataSource = receiver });
                return container;
            });

            return receiver;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
