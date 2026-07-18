// <copyright file="IAnchorToolbarHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Toolbar
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Contract for toolbar host implementations so core systems can remain AppUI-independent.
    /// </summary>
    public interface IAnchorToolbarHost
    {
        /// <summary>Creates a fresh root visual element for the toolbar UI.</summary>
        /// <returns>The newly composed toolbar root.</returns>
        VisualElement CreateRootVisualElement();
    }
}
