namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    public partial class AnchorGridView : GridView
    {
        public AnchorGridView()
        {
            makeItem = MakeItem;
            bindItem = BindItem;
        }

        [UxmlAttribute]
        public VisualTreeAsset itemTemplate { get; set; }

        private VisualElement MakeItem()
        {
            return itemTemplate != null ? itemTemplate.Instantiate() : new Label("Template Not Found");
        }

        private void BindItem(VisualElement element, int index)
        {
            element.dataSource = itemsSource[index];
        }
    }
}
