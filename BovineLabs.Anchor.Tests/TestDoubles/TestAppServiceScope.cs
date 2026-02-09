// <copyright file="TestAppServiceScope.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using Unity.AppUI.MVVM;

    internal sealed class TestAppServiceScope : IDisposable
    {
        private readonly App app;
        private readonly ServiceProvider serviceProvider;

        public TestAppServiceScope(params Type[] singletonServiceTypes)
        {
            if (App.current != null)
            {
                throw new InvalidOperationException("App.current must be null before creating a test app scope.");
            }

            var services = new ServiceCollection();
            foreach (var singletonType in singletonServiceTypes)
            {
                services.AddSingleton(singletonType);
            }

            this.serviceProvider = services.BuildServiceProvider();
            this.app = new App();
            this.app.Initialize(this.serviceProvider);
        }

        public void Dispose()
        {
            this.app.Dispose();
            this.serviceProvider.Dispose();
        }
    }
}
