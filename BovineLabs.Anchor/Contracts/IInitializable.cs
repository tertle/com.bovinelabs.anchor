// <copyright file="IInitializable.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    /// <summary>
    /// Contract for services that require an explicit initialization step before use.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>Performs one-time setup logic.</summary>
        void Initialize();
    }
}
