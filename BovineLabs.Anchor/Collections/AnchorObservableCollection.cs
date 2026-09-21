namespace BovineLabs.Anchor.Collections
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Unity.Properties;

    /// <summary>
    /// Register the collection with PropertyBag.RegisterIList to enable property binding.
    /// </summary>
    public class AnchorObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Raises one Reset notification. Null leaves the collection unchanged.
        /// </summary>
        public void Replace(IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            var snapshot = new List<T>(items);
            if (Items.Count == 0 && snapshot.Count == 0)
            {
                return;
            }

            Items.Clear();

            foreach (var item in snapshot)
            {
                Items.Add(item);
            }

            OnBulkCollectionChanged();
        }

        /// <summary>
        /// Raises one Reset notification if items were added. Null leaves the collection unchanged.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            var snapshot = new List<T>(items);
            if (snapshot.Count == 0)
            {
                return;
            }

            foreach (var item in snapshot)
            {
                Items.Add(item);
            }

            OnBulkCollectionChanged();
        }

        private void OnBulkCollectionChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
