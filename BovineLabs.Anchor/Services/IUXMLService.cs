// <copyright file="IUXMLService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine.UIElements;

    public interface IUXMLService
    {
        VisualTreeAsset GetAsset(string assetName);

        VisualElement Instantiate(string assetName);

        void CloneTree(string assetName, VisualElement target);
    }
}
