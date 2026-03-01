// <copyright file="AnchorAppUIPanel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Adapters
{
    using BovineLabs.Anchor;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// AppUI-backed panel implementation for Anchor when AppUI is available.
    /// </summary>
    public class AnchorAppUIPanel : Panel, IAnchorPanel
    {
        /// <inheritdoc />
        public VisualElement RootVisualElement => this;

        /// <inheritdoc/>
        public string Theme
        {
            get => this.theme;
            set => this.theme = value;
        }

        /// <inheritdoc/>
        public string Scale
        {
            get => this.scale;
            set => this.scale = value;
        }
    }
}
