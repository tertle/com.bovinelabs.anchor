// <copyright file="TestAnchorAppScope.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor;
    using Unity.AppUI.MVVM;

    internal sealed class TestAnchorAppScope : IDisposable
    {
        private readonly AnchorApp app;
        private readonly ServiceProvider serviceProvider;

        public TestAnchorAppScope(Action<ServiceCollection> configureServices = null)
        {
            if (Unity.AppUI.MVVM.App.current != null)
            {
                throw new InvalidOperationException("App.current must be null before creating a test app scope.");
            }

            var services = new ServiceCollection();
            configureServices?.Invoke(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.app = new AnchorApp();
            this.app.Initialize(this.serviceProvider);
        }

        public ServiceProvider ServiceProvider => this.serviceProvider;

        public AnchorApp App => this.app;

        public void Dispose()
        {
            this.app.Dispose();
            this.serviceProvider.Dispose();
        }
    }
}
