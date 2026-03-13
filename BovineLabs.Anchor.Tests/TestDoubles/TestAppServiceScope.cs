// <copyright file="TestAppServiceScope.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor.MVVM;

    internal sealed class TestAppServiceScope : IDisposable
    {
        private readonly AnchorApp app;
        private readonly AnchorServiceProvider serviceProvider;

        public TestAppServiceScope(params Type[] singletonServiceTypes)
        {
            if (AnchorApp.Current != null)
            {
                throw new InvalidOperationException("AnchorApp.Current must be null before creating a test app scope.");
            }

            var services = new AnchorServiceCollection();
            foreach (var singletonType in singletonServiceTypes)
            {
                services.AddSingleton(singletonType);
            }

            this.serviceProvider = services.BuildServiceProvider();
            this.app = new AnchorApp();
            this.app.Initialize(this.serviceProvider, new AnchorPanel());
        }

        public void Dispose()
        {
            this.app.Dispose();
            this.serviceProvider.Dispose();
        }
    }
}

