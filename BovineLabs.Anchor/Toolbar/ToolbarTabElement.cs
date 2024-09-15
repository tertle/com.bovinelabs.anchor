// <copyright file="ToolbarTabElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public partial class ToolbarTabElement : VisualElement
    {
        private const string UssClassName = "bl-toolbar-tab";

        private const string NameClass = UssClassName + "__name";

        private readonly VisualElement content;

        private readonly Heading groupLabel;

        public ToolbarTabElement(string label)
        {
            this.AddToClassList(UssClassName);

            this.content = new VisualElement();
            this.hierarchy.Add(this.content);

            this.groupLabel = new Heading(label) { size = HeadingSize.XXS };
            this.groupLabel.AddToClassList(NameClass);
            this.hierarchy.Add(this.groupLabel);
        }

        public override VisualElement contentContainer => this.content;

        [UxmlAttribute]
        [CreateProperty]
        public string label
        {
            get => this.groupLabel.text;
            set => this.groupLabel.text = value;
        }
    }
}
#endif
