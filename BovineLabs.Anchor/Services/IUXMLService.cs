// <copyright file="IUXMLService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Resolves registered UXML assets and instantiates them on demand.
    /// </summary>
    public interface IUXMLService
    {
        /// <summary>Retrieves a visual tree asset by key.</summary>
        /// <param name="assetName">Identifier of the asset to load.</param>
        /// <returns>The registered visual tree asset.</returns>
        VisualTreeAsset GetAsset(string assetName);

        /// <summary>Instantiates a new visual tree for the requested asset.</summary>
        /// <param name="assetName">Identifier of the asset to clone.</param>
        /// <returns>A new visual element tree.</returns>
        VisualElement Instantiate(string assetName);
    }
}
