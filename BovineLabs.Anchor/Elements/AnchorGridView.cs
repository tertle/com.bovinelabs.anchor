// <copyright file="AnchorGridView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorGridView : GridView
    {
        private static readonly BindingId SelectedIndexProperty = new(nameof(selectedIndex));
        private static readonly BindingId SelectedIndicesProperty = new(nameof(selectedIndices));
        private static readonly BindingId ItemsSourceProperty = new(nameof(itemsSource));
        private static readonly BindingId ColumnCountProperty = new(nameof(columnCount));

        public AnchorGridView()
        {
            this.makeItem = this.MakeItem;
            this.bindItem = this.BindItem;

            this.selectionChanged += _ => this.NotifyPropertyChanged(SelectedIndexProperty);
            this.selectedIndicesChanged += _ => this.NotifyPropertyChanged(SelectedIndicesProperty);

            this.itemsSource = Array.Empty<object>();
        }

        /// <summary>
        /// Gets or sets the visual tree asset cloned for each slot.
        /// </summary>
        [UxmlAttribute]
        public VisualTreeAsset itemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the number of columns for this grid.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public new int columnCount
        {
            get => base.columnCount;

            set
            {
                if (base.columnCount == value)
                {
                    return;
                }

                base.columnCount = value;
                this.NotifyPropertyChanged(ColumnCountProperty);
            }
        }

        /// <summary>
        /// Returns or sets the selected item's index in the data source. If multiple items are selected, returns the
        /// first selected item's index. If multiple items are provided, sets them all as selected.
        /// </summary>
        [CreateProperty]
        public new int selectedIndex
        {
            get => base.selectedIndex;
            set
            {
                if (base.selectedIndex == value)
                {
                    return;
                }

                base.selectedIndex = value;
            }
        }

        /// <summary>
        /// Returns the indices of selected items in the data source. Always returns an enumerable, even if no item  is selected, or a
        /// single item is selected.
        /// </summary>
        [CreateProperty]
        public new IEnumerable<int> selectedIndices => base.selectedIndices;

        /// <summary>
        /// The data source for list items.
        /// </summary>
        /// <remarks>
        /// This list contains the items that the <see cref="GridView"/> displays.
        ///
        /// This property must be set for the list view to function.
        /// </remarks>
        [CreateProperty]
        public new IList itemsSource
        {
            get => base.itemsSource;
            set
            {
                if (ReferenceEquals(base.itemsSource, value))
                {
                    return;
                }

                base.itemsSource = value;
                this.NotifyPropertyChanged(ItemsSourceProperty);
            }
        }

        /// <summary>
        /// Callback for constructing the VisualElement that is the template for each recycled and re-bound element in the list.
        /// </summary>
        /// <remarks>
        /// This callback needs to call a function that constructs a blank <see cref="VisualElement"/> that is
        /// bound to an element from the list.
        ///
        /// The GridView automatically creates enough elements to fill the visible area, and adds more if the area
        /// is expanded. As the user scrolls, the GridView cycles elements in and out as they appear or disappear.
        ///
        ///  This property must be set for the list view to function.
        /// </remarks>
        [CreateProperty]
        public new Func<VisualElement> makeItem
        {
            get => base.makeItem;
            set => base.makeItem = value;
        }

        /// <summary>
        /// Callback for binding a data item to the visual element.
        /// </summary>
        /// <remarks>
        /// The method called by this callback receives the VisualElement to bind, and the index of the
        /// element to bind it to.
        /// </remarks>
        [CreateProperty]
        public new Action<VisualElement, int> bindItem
        {
            get => base.bindItem;
            set => base.bindItem = value;
        }

        private VisualElement MakeItem()
        {
            return this.itemTemplate != null ? this.itemTemplate.Instantiate() : new Label("Template Not Found");
        }

        private void BindItem(VisualElement element, int index)
        {
            element.dataSource = this.itemsSource;
            element.dataSourcePath = PropertyPath.FromIndex(index);
        }
    }
}
