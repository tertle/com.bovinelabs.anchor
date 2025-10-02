// <copyright file="AnchorObservableCollection.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Collections
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using BovineLabs.Anchor.Elements;
    using Unity.Properties;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> with support for bulk operations.
    /// This is especially useful for UI elements such as <see cref="AnchorGridView"/> that rely on
    /// <see cref="INotifyCollectionChanged"/> but should only update once when multiple items are modified.
    ///
    /// Example usage as an indexed binding:
    /// <code>
    /// element.dataSource = this.itemsSource;
    /// element.dataSourcePath = PropertyPath.FromIndex(index);
    /// </code>
    ///
    /// To enable property binding, register the collection type with Unity’s property system:
    /// <see cref="PropertyBag.RegisterIList{TList,TElement}"/>
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public class AnchorObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Replaces the entire contents of the collection with the specified items, raising a single
        /// <see cref="NotifyCollectionChangedAction.Reset"/> event instead of per-item notifications.
        /// </summary>
        /// <param name="items">The collection of items to replace the current contents with. If null, the collection remains unchanged.</param>
        public void Replace(IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            var wasEmpty = this.Items.Count == 0;

            this.Items.Clear();

            foreach (var item in items)
            {
                this.Items.Add(item);
            }

            // Check it's not still empty
            if (!wasEmpty || this.Items.Count != 0)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Adds a range of items to the collection in one operation.
        /// If any items are added, only a single <see cref="NotifyCollectionChangedAction.Reset"/> event is raised.
        /// </summary>
        /// <param name="items">The collection of items to add. If null, no items are added.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            var count = this.Items.Count;

            foreach (var item in items)
            {
                this.Items.Add(item);
            }

            if (this.Items.Count != count)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
