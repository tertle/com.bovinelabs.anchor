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
            if (this.Items.Count == 0 && snapshot.Count == 0)
            {
                return;
            }

            this.Items.Clear();

            foreach (var item in snapshot)
            {
                this.Items.Add(item);
            }

            this.OnBulkCollectionChanged();
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
                this.Items.Add(item);
            }

            this.OnBulkCollectionChanged();
        }

        private void OnBulkCollectionChanged()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Count)));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
