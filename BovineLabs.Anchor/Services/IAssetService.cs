// <copyright file="IAssetService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine;

    public interface IAssetService
    {
        Object Get(string key);
    }
}
