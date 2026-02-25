// <copyright file="TestAnchorNavHostHarness.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using UnityEngine.UIElements;

    internal sealed class TestAnchorNavHostHarness : IDisposable
    {
        private readonly TestAnchorAppScope scope;

        public TestAnchorNavHostHarness()
        {
            this.scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            this.Factory = this.scope.ServiceProvider.GetService(typeof(TestVisualElementFactory)) as TestVisualElementFactory;
            if (this.Factory == null)
            {
                throw new InvalidOperationException("Failed to resolve TestVisualElementFactory.");
            }

            this.Host = new AnchorNavHost();
            this.scope.App.NavHost = this.Host;
        }

        public AnchorNavHost Host { get; }

        private TestVisualElementFactory Factory { get; }

        public TestNavigationScreenReceiver RegisterScreen(string destination)
        {
            var receiver = new TestNavigationScreenReceiver();

            this.Factory.Register(destination, () =>
            {
                var container = new VisualElement { name = destination };
                container.Add(new VisualElement { dataSource = receiver });
                return container;
            });

            return receiver;
        }

        public void Dispose()
        {
            this.scope.Dispose();
        }
    }
}
