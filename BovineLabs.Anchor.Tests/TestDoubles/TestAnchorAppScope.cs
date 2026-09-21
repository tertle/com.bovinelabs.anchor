namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;

    internal sealed class TestAnchorAppScope : IDisposable
    {
        private readonly AnchorApp _app;
        private readonly AnchorServiceProvider _serviceProvider;

        public TestAnchorAppScope(Action<AnchorServiceCollection> configureServices = null)
        {
            if (AnchorApp.Current != null)
            {
                throw new InvalidOperationException("AnchorApp.Current must be null before creating a test app scope.");
            }

            var services = new AnchorServiceCollection();
            configureServices?.Invoke(services);

            _serviceProvider = services.BuildServiceProvider();
            _app = new AnchorApp();
            _app.Initialize(_serviceProvider);
            _app.SetPanel(new AnchorPanel());
        }

        public AnchorServiceProvider ServiceProvider => _serviceProvider;

        public AnchorApp App => _app;

        public void Dispose()
        {
            _app.Dispose();
            _serviceProvider.Dispose();
        }
    }
}

