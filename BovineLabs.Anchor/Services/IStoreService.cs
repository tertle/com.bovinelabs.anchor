// <copyright file="IStoreService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using Unity.AppUI.Redux;

    public interface IStoreService
    {
        IStore<PartitionedState> Store { get; }
    }
}
