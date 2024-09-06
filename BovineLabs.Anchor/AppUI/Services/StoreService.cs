// <copyright file="StoreService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using Unity.AppUI.Redux;

    public record StoreService : IStoreService
    {
        public Store Store { get; } = new();
    }
}
