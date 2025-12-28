// <copyright file="UXMLService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using BovineLabs.Anchor;
    using BovineLabs.Core;
    using JetBrains.Annotations;
    using UnityEngine.UIElements;

    /// <summary>
    /// Default implementation that looks up view templates from <see cref="AnchorSettings"/>.
    /// </summary>
    [UsedImplicitly]
    public class UXMLService : IUXMLService
    {
        /// <inheritdoc/>
        public VisualTreeAsset GetAsset(string assetName)
        {
            foreach (var v in AnchorSettings.I.Views)
            {
                if (v.Key == assetName)
                {
                    return v.Asset;
                }
            }

            BLGlobalLogger.LogError($"VisualTreeAsset for the key {assetName} was not found. Check AnchorSettings.");
            return null;
        }

        /// <inheritdoc/>
        public VisualElement Instantiate(string assetName)
        {
            var asset = this.GetAsset(assetName);
            var container = asset.Instantiate();
            container.pickingMode = PickingMode.Ignore;

            foreach (var ve in container.Query().Build())
            {
                if (ve.dataSourceType != null)
                {
                    ve.dataSource = AnchorApp.current.services.GetService(ve.dataSourceType);
                }
            }

            return container;
        }
    }
}
