// <copyright file="AnchorPanel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    /// <summary>
    /// AppUI-backed panel implementation for Anchor.
    /// </summary>
    [Preserve]
    public class AnchorPanel : Panel, IAnchorPanel
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
