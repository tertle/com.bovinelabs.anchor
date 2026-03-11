// <copyright file="AnchorPanel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
#if UNITY_APPUI
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// AppUI-backed panel implementation for Anchor when AppUI is available.
    /// </summary>
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
#else
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
#endif
}
