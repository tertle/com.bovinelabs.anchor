// <copyright file="UXMLService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor;
    using BovineLabs.Core;
    using JetBrains.Annotations;
    using UnityEngine.UIElements;

    [UsedImplicitly]
    public class UXMLService : IUXMLService
    {
        private readonly Dictionary<string, VisualTreeAsset> assets;

        public UXMLService()
        {
            this.assets = AnchorSettings.I.Views.ToDictionary(t => t.Key, t => t.Asset);
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            try
            {
                return this.assets[assetName];
            }
            catch (KeyNotFoundException)
            {
                BLGlobalLogger.LogError($"VisualTreeAsset for the key {assetName} was not found. Check AnchorSettings.");
                throw;
            }
        }

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