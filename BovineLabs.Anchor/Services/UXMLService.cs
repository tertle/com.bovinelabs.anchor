namespace BovineLabs.Anchor.Services
{
    using System;
    using BovineLabs.Anchor;
    using BovineLabs.Core;
    using JetBrains.Annotations;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [UsedImplicitly]
    public class UXMLService : IUXMLService
    {
        public VisualTreeAsset GetAsset(string assetName)
        {
            VisualTreeAsset asset = null;

            foreach (var v in AnchorSettings.I.Views)
            {
                if (v.Key == assetName)
                {
                    asset = v.Asset;
                    break;
                }
            }

            if (asset == null)
            {
                BLGlobalLogger.LogErrorString($"VisualTreeAsset for the key {assetName} was not found or null. Check AnchorSettings.");
            }

            return asset;
        }

        public VisualElement Instantiate(string assetName)
        {
            var asset = GetAsset(assetName);
            if (asset == null)
            {
                return new VisualElement();
            }

            var container = asset.Instantiate();

            foreach (var ve in container.Query().Build())
            {
                if (ve.dataSourceType != null)
                {
                    ve.dataSource = AnchorApp.Current.Services.GetService(ve.dataSourceType);
                }
            }

            return container;
        }
    }
}

