namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorGridView : GridView
    {
        public AnchorGridView()
        {
            this.makeItem = this.MakeItem;
            this.bindItem = this.BindItem;
        }

        [UxmlAttribute]
        public VisualTreeAsset itemTemplate { get; set; }

        private VisualElement MakeItem()
        {
            return this.itemTemplate != null ? this.itemTemplate.Instantiate() : new Label("Template Not Found");
        }

        private void BindItem(VisualElement element, int index)
        {
            element.dataSource = this.itemsSource[index];
        }
    }
}
