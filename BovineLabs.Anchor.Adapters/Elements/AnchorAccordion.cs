// <copyright file="AnchorAccordion.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Accordion that repeats a UXML template for each bound data item.
    /// </summary>
    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorAccordion : Accordion
    {
        public static readonly BindingId ItemTemplateProperty = (BindingId)nameof(itemTemplate);
        public static readonly BindingId ItemsSourceProperty = (BindingId)nameof(itemsSource);

        private VisualTreeAsset m_ItemTemplate;
        private IList m_ItemsSource;

        /// <summary>Gets or sets the template cloned for each accordion item.</summary>
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

        /// <summary>Gets or sets the collection of data objects that populate the accordion.</summary>
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
