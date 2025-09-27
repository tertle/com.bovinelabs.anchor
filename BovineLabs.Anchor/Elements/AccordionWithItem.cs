// <copyright file="AccordionWithItem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Collections;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AccordionWithItem : Accordion
    {
        public static readonly BindingId ItemTemplateProperty = (BindingId)nameof(itemTemplate);
        public static readonly BindingId ItemsSourceProperty = (BindingId)nameof(itemsSource);

        private VisualTreeAsset m_ItemTemplate;
        private IList m_ItemsSource;

        [CreateProperty]
        [UxmlAttribute]
        public VisualTreeAsset itemTemplate
        {
            get => this.m_ItemTemplate;
            set
            {
                if (this.m_ItemTemplate == value)
                {
                    return;
                }

                this.m_ItemTemplate = value;
                this.Rebuild();
                this.NotifyPropertyChanged(ItemTemplateProperty);
            }
        }

        [CreateProperty]
        public IList itemsSource
        {
            get => this.m_ItemsSource;
            set
            {
                if (ReferenceEquals(this.m_ItemsSource, value))
                {
                    return;
                }

                this.m_ItemsSource = value;
                this.Rebuild();
                this.NotifyPropertyChanged(ItemsSourceProperty);
            }
        }

        private void Rebuild()
        {
            this.Clear();

            if (this.itemTemplate == null || this.itemsSource == null)
            {
                return;
            }

            for (var index = 0; index < this.itemsSource.Count; index++)
            {
                this.itemTemplate.CloneTree(this);
            }

            var i = 0;
            foreach (var item in this.Query<AccordionItem>().Build())
            {
                item.dataSource = this.itemsSource[i++];
            }
        }
    }
}
