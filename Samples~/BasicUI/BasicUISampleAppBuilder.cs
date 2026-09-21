namespace BovineLabs.Anchor.Samples.BasicUI
{
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class BasicUISampleAppBuilder : AnchorAppBuilder, IUXMLService
    {
        private const string Destination = "anchor-basic-ui";

        [SerializeField]
        private VisualTreeAsset _view;

        protected override void OnConfigureServices(AnchorServiceCollection services)
        {
            base.OnConfigureServices(services);
            services.AddSingletonInstance(typeof(IUXMLService), this);
        }

        protected override void OnVisualGenerationInitialized(AnchorApp app)
        {
            base.OnVisualGenerationInitialized(app);
            app.NavHost.Navigate(Destination);
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            return _view;
        }

        public VisualElement Instantiate(string assetName)
        {
            return _view.Instantiate();
        }
    }
}
