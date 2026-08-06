// <copyright file="BasicUISampleAppBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Samples.BasicUI
{
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Builds the sample app and opens its UXML screen.</summary>
    public sealed class BasicUISampleAppBuilder : AnchorAppBuilder, IUXMLService
    {
        private const string Destination = "anchor-basic-ui";

        [SerializeField]
        private VisualTreeAsset view;

        /// <inheritdoc />
        protected override void OnConfigureServices(AnchorServiceCollection services)
        {
            base.OnConfigureServices(services);
            services.AddSingletonInstance(typeof(IUXMLService), this);
        }

        /// <inheritdoc />
        protected override void OnVisualGenerationInitialized(AnchorApp app)
        {
            base.OnVisualGenerationInitialized(app);
            app.NavHost.Navigate(Destination);
        }

        /// <inheritdoc />
        public VisualTreeAsset GetAsset(string assetName)
        {
            return this.view;
        }

        /// <inheritdoc />
        public VisualElement Instantiate(string assetName)
        {
            return this.view.Instantiate();
        }
    }
}
