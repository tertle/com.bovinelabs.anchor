// <copyright file="IStoreService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.Redux;

    public interface IStoreService
    {
        Store Store { get; }
    }
}
