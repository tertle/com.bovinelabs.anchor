// <copyright file="ToolbarGroupElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using UnityEngine.UIElements;

    [UxmlElement]
    public sealed partial class ToolbarGroupElement : ScrollView
    {
        public const string UssClassName = "bl-toolbar-group";

        public ToolbarGroupElement()
        {
            this.AddToClassList(UssClassName);

            this.mode = ScrollViewMode.Horizontal;
            this.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            this.focusable = false;

            this.horizontalScroller.RemoveFromHierarchy();
        }

        public void AddToTab(VisualElement tab)
        {
            tab.Add(this);
            tab.Add(this.horizontalScroller);
        }

        public void RemoveFromTab()
        {
            this.RemoveFromHierarchy();
            this.horizontalScroller.RemoveFromHierarchy();
        }
    }
}
#endif
