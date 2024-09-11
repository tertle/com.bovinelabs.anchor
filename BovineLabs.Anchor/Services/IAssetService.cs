// <copyright file="IAssetService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine;

    public interface IAssetService
    {
        bool TryGet<T>(string key, out T obj)
            where T : Object;
    }
}
