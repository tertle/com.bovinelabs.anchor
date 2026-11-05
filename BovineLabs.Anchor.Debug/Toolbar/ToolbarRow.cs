namespace BovineLabs.Anchor.Debug.Toolbar
{
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// A compact toolbar row with a label on the left and its control on the right.
    /// </summary>
    public class ToolbarRow : VisualElement
    {
        public const string UssClassName = "bl-toolbar-row";
        public const string LabelUssClassName = UssClassName + "__label";

        public ToolbarRow(string label, VisualElement control)
        {
            AddToClassList(UssClassName);

            var text = new Text(label);
            text.AddToClassList(LabelUssClassName);
            Add(text);
            Add(control);
        }
    }
}
