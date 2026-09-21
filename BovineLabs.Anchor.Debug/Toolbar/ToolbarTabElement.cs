namespace BovineLabs.Anchor.Debug.Toolbar
{
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public sealed class ToolbarTabElement : VisualElement
    {
        private const string UssClassName = "bl-toolbar-tab";
        private const string NameClass = UssClassName + "__name";

        private readonly VisualElement _content;

        public ToolbarTabElement(string label)
        {
            AddToClassList(UssClassName);

            _content = new VisualElement();
            hierarchy.Add(_content);

            var groupLabel = new Heading(label) { size = HeadingSize.XXS };
            groupLabel.AddToClassList(NameClass);
            hierarchy.Add(groupLabel);

            focusable = false;
        }

        public override VisualElement contentContainer => _content;
    }
}
