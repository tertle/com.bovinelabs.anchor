// <copyright file="IToolbarElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Durable toolbar model capable of creating a fresh visual projection for each toolbar generation.
    /// </summary>
    public interface IToolbarElement
    {
        /// <summary>Creates a new toolbar element bound to this model.</summary>
        /// <returns>A fresh visual element that has not previously been attached to a toolbar.</returns>
        VisualElement CreateElement();
    }
}
