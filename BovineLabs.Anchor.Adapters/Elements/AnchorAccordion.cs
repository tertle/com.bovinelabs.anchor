namespace BovineLabs.Anchor.Elements
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    public partial class AnchorAccordion : Accordion
    {
        public static readonly BindingId ItemTemplateProperty = (BindingId)nameof(itemTemplate);
        public static readonly BindingId ItemsSourceProperty = (BindingId)nameof(itemsSource);

        private VisualTreeAsset _itemTemplate;
        private IList _itemsSource;

        [CreateProperty]
        [UxmlAttribute]
        public VisualTreeAsset itemTemplate
        {
            get => _itemTemplate;
            set
            {
                if (_itemTemplate == value)
                {
                    return;
                }

                _itemTemplate = value;
                Rebuild();
                NotifyPropertyChanged(ItemTemplateProperty);
            }
        }

        [CreateProperty]
        public IList itemsSource
        {
            get => _itemsSource;
            set
            {
                if (ReferenceEquals(_itemsSource, value))
                {
                    return;
                }

                _itemsSource = value;
                Rebuild();
                NotifyPropertyChanged(ItemsSourceProperty);
            }
        }

        private void Rebuild()
        {
            Clear();

            if (itemTemplate == null || itemsSource == null)
            {
                return;
            }

            for (var index = 0; index < itemsSource.Count; index++)
            {
                itemTemplate.CloneTree(this);
            }

            var i = 0;
            foreach (var item in this.Query<AccordionItem>().Build())
            {
                item.dataSource = itemsSource[i++];
            }
        }
    }
}
