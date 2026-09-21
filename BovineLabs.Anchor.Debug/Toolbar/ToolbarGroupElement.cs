namespace BovineLabs.Anchor.Debug.Toolbar
{
    using UnityEngine.UIElements;

    [UxmlElement]
    public sealed partial class ToolbarGroupElement : ScrollView
    {
        public const string UssClassName = "bl-toolbar-group";

        public ToolbarGroupElement()
        {
            AddToClassList(UssClassName);

            mode = ScrollViewMode.Horizontal;
            verticalScrollerVisibility = ScrollerVisibility.Hidden;
            focusable = false;

            horizontalScroller.RemoveFromHierarchy();
        }

        public void AddToTab(VisualElement tab)
        {
            tab.Add(this);
            tab.Add(horizontalScroller);
        }

        public void RemoveFromTab()
        {
            RemoveFromHierarchy();
            horizontalScroller.RemoveFromHierarchy();
        }
    }
}
