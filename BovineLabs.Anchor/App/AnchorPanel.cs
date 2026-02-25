// <copyright file="AnchorPanel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Root panel element used by Anchor apps.
    /// </summary>
    public sealed class AnchorPanel : VisualElement, IAnchorPanel
    {
        /// <inheritdoc />
        public VisualElement RootVisualElement => this;

        /// <inheritdoc />
        public string Scale { get; set; } = "medium";

        /// <inheritdoc />
        public string Theme { get; set; }
    }
}
