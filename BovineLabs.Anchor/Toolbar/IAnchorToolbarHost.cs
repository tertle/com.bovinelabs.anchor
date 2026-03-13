// <copyright file="IAnchorToolbarHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// Contract for toolbar host implementations so core systems can remain AppUI-independent.
    /// </summary>
    public interface IAnchorToolbarHost
    {
        /// <summary>Gets the root visual element for the toolbar UI.</summary>
        VisualElement RootVisualElement { get; }

        /// <summary>
        /// Adds a toolbar tab entry.
        /// </summary>
        /// <param name="viewType">View type to resolve from services.</param>
        /// <param name="tabName">Tab name.</param>
        /// <param name="elementName">Element/group name.</param>
        /// <param name="id">Allocated tab id.</param>
        /// <param name="view">Resolved view instance.</param>
        void AddTab(Type viewType, string tabName, string elementName, out int id, out VisualElement view);

        /// <summary>
        /// Removes a previously added tab entry.
        /// </summary>
        /// <param name="id">Tab id.</param>
        /// <returns>Removed view or null.</returns>
        VisualElement RemoveTab(int id);
    }
}
