// <copyright file="TestUxmlService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using BovineLabs.Anchor.Services;
    using UnityEngine.UIElements;

    internal sealed class TestUxmlService : IUXMLService
    {
        private readonly TestVisualElementFactory visualElementFactory;

        public TestUxmlService(TestVisualElementFactory visualElementFactory)
        {
            this.visualElementFactory = visualElementFactory;
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            return null;
        }

        public VisualElement Instantiate(string assetName)
        {
            var element = this.visualElementFactory.Create(assetName);
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }
}
