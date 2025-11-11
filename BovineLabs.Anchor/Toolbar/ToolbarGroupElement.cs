// <copyright file="ToolbarGroupElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Scroll view that hosts toolbar groups inside a tab.
    /// </summary>
    [UxmlElement]
    public sealed partial class ToolbarGroupElement : ScrollView
    {
        public const string UssClassName = "bl-toolbar-group";

        /// <summary>Initializes a new instance of the <see cref="ToolbarGroupElement"/> class.</summary>
        public ToolbarGroupElement()
        {
            this.AddToClassList(UssClassName);

            this.mode = ScrollViewMode.Horizontal;
            this.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            this.focusable = false;

            this.horizontalScroller.RemoveFromHierarchy();
        }

        /// <summary>Adds the group to the supplied tab container.</summary>
        public void AddToTab(VisualElement tab)
        {
            tab.Add(this);
            tab.Add(this.horizontalScroller);
        }

        /// <summary>Removes the group and its scroller from the UI hierarchy.</summary>
        public void RemoveFromTab()
        {
            this.RemoveFromHierarchy();
            this.horizontalScroller.RemoveFromHierarchy();
        }
    }
}
#endif
