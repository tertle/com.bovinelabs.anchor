namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor.MVVM;

    internal sealed class TestAppServiceScope : IDisposable
    {
        private readonly AnchorApp _app;
        private readonly AnchorServiceProvider _serviceProvider;

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

            _serviceProvider = services.BuildServiceProvider();
            _app = new AnchorApp();
            _app.Initialize(_serviceProvider);
            _app.SetPanel(new AnchorPanel());
        }

        public void Dispose()
        {
            _app.Dispose();
            _serviceProvider.Dispose();
        }
    }
}

