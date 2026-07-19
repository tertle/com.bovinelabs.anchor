// <copyright file="AnchorPanel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
#if UNITY_APPUI
    using Unity.AppUI.UI;

    /// <summary>
    /// AppUI-backed panel implementation for Anchor when AppUI is available.
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
#else
    /// <summary>
    /// Root panel element used by Anchor apps.
    /// </summary>
    [Preserve]
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
