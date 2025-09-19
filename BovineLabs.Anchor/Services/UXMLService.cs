// <copyright file="UXMLService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
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
            this.assets = UXMLServiceAssets.I.Values.ToDictionary(t => t.Key, t => t.Asset);
        }

        public VisualTreeAsset GetAsset(string assetName)
        {
            try
            {
                return this.assets[assetName];
            }
            catch (KeyNotFoundException)
            {
                BLGlobalLogger.LogError($"Can't find key {assetName}");
                throw;
            }
        }

        public VisualElement Instantiate(string assetName)
        {
            var asset = this.GetAsset(assetName);
            var container = asset.Instantiate();
            container.pickingMode = PickingMode.Ignore;

            foreach (var ve in container.Children())
            {
                if (ve.dataSourceType != null)
                {
                    ve.dataSource = AnchorApp.current.services.GetService(ve.dataSourceType);
                }
            }

            return container;
        }

        public void CloneTree(string assetName, VisualElement target)
        {
            var asset = this.GetAsset(assetName);
            asset.CloneTree(target);

            foreach (var ve in target.Children())
            {
                if (ve.dataSource == null && ve.dataSourceType != null)
                {
                    ve.dataSource = AnchorApp.current.services.GetService(ve.dataSourceType);
                }
            }
        }
    }
}
#endif