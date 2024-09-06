// <copyright file="AssetService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AssetService : IAssetService
    {
        private readonly Dictionary<string, Object> objects = new();

        public AssetService(GameObject gameObject)
        {
            var assets = gameObject.GetComponent<AssetList>();
            if (assets == null)
            {
                return;
            }

            foreach (var asset in assets.Assets)
            {
                if (asset.Asset == null)
                {
                    continue;
                }

                this.objects.Add(asset.Key, asset.Asset);
            }
        }

        public Object Get(string key)
        {
            if (!this.objects.TryGetValue(key, out var asset))
            {
                throw new ArgumentException($"Key ({key}) not found", nameof(key));
            }

            return asset;
        }
    }
}
