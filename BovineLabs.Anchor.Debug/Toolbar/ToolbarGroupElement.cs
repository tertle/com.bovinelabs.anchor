namespace BovineLabs.Anchor.Debug.Toolbar
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
