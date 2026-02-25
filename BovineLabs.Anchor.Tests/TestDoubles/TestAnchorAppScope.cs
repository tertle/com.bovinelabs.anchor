// <copyright file="TestAnchorAppScope.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;

    internal sealed class TestAnchorAppScope : IDisposable
    {
        private readonly AnchorApp app;
        private readonly AnchorServiceProvider serviceProvider;

        public TestAnchorAppScope(Action<AnchorServiceCollection> configureServices = null)
        {
            if (AnchorApp.Current != null)
            {
                throw new InvalidOperationException("AnchorApp.Current must be null before creating a test app scope.");
            }

            var services = new AnchorServiceCollection();
            configureServices?.Invoke(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.app = new AnchorApp();
            this.app.Initialize(this.serviceProvider, new AnchorPanel());
        }

        public AnchorServiceProvider ServiceProvider => this.serviceProvider;

        public AnchorApp App => this.app;

        public void Dispose()
        {
            this.app.Dispose();
            this.serviceProvider.Dispose();
        }
    }
}

