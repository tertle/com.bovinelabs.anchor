namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using BovineLabs.Anchor.Services;
    using UnityEngine.UIElements;

    internal sealed class TestUxmlService : IUXMLService
    {
        private readonly TestVisualElementFactory _visualElementFactory;

        public TestUxmlService(TestVisualElementFactory visualElementFactory)
        {
            _visualElementFactory = visualElementFactory;
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            return null;
        }

        public VisualElement Instantiate(string assetName)
        {
            return _visualElementFactory.Create(assetName);
        }
    }
}
