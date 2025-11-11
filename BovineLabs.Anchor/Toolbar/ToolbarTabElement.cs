// <copyright file="ToolbarTabElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// Container that wraps toolbar content with a labeled heading.
    /// </summary>
    public sealed class ToolbarTabElement : VisualElement
    {
        private const string UssClassName = "bl-toolbar-tab";
        private const string NameClass = UssClassName + "__name";

        private readonly VisualElement content;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarTabElement"/> class.
        /// </summary>
        /// <param name="label">Heading displayed above the tab content.</param>
        public ToolbarTabElement(string label)
        {
            this.AddToClassList(UssClassName);

            this.content = new VisualElement();
            this.hierarchy.Add(this.content);

            var groupLabel = new Heading(label) { size = HeadingSize.XXS };
            groupLabel.AddToClassList(NameClass);
            this.hierarchy.Add(groupLabel);

            focusable = false;
        }

        /// <summary>Gets the container that should receive dynamically added content.</summary>
        public override VisualElement contentContainer => this.content;
    }
}
#endif
