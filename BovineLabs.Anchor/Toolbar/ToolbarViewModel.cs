// <copyright file="ToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Toolbar
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.Properties;

    /// <summary>
    /// View model that keeps track of toolbar filter selections and persistence.
    /// </summary>
    [IsService]
    public class ToolbarViewModel : ObservableObject
    {
        private const string SelectionKey = "bl.toolbarmanager.filter.selections";

        private readonly ILocalStorageService storageService;
        private readonly Dictionary<string, int> selectionsCount = new();
        private readonly HashSet<string> selectionsHidden = new();
        private readonly List<int> filterValuesPrevious = new();
        private readonly List<int> filterValues = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarViewModel"/> class.
        /// </summary>
        /// <param name="storageService">Service used to persist filter selections.</param>
        public ToolbarViewModel(ILocalStorageService storageService)
        {
            this.storageService = storageService;

            var selectionSaved = storageService.GetValue(SelectionKey, string.Empty);
            var selectionArray = selectionSaved.Split(",");
            this.selectionsHidden.UnionWith(selectionArray);
            this.selectionsHidden.Remove(string.Empty);
        }

        /// <summary>Gets the set of group names currently hidden by the filter dropdown.</summary>
        public IReadOnlyCollection<string> SelectionsHidden => this.selectionsHidden;

        /// <summary>Gets the ordered list of filter names shown in the dropdown.</summary>
        public List<string> FilterItems { get; } = new();

        /// <summary>Gets or sets the indices of filter items that are currently selected.</summary>
        [CreateProperty]
        public IEnumerable<int> FilterValues
        {
            get => this.filterValues;
            set
            {
                if (SequenceComparer.Int.Equals(this.filterValues, value))
                {
                    return;
                }

                this.OnPropertyChanging();

                this.filterValues.Clear();
                this.filterValues.AddRange(value);

                foreach (var oldValue in this.filterValuesPrevious.Where(oldValue => !this.filterValues.Contains(oldValue)))
                {
                    this.selectionsHidden.Add(this.FilterItems[oldValue]);
                }

                foreach (var newValue in this.filterValues.Where(newValue => !this.filterValuesPrevious.Contains(newValue)))
                {
                    this.selectionsHidden.Remove(this.FilterItems[newValue]);
                }

                var serializedString = string.Join(",", this.selectionsHidden);
                this.storageService.SetValue(SelectionKey, serializedString);

                this.filterValuesPrevious.Clear();
                this.filterValuesPrevious.AddRange(this.filterValues);

                this.OnPropertyChanged();
            }
        }

        /// <summary>Registers that a toolbar group with the specified filter name is now available.</summary>
        /// <param name="filterName">Name used to identify the toolbar group.</param>
        public void AddSelection(string filterName)
        {
            this.selectionsCount.TryGetValue(filterName, out var count);
            if (count == 0)
            {
                this.FilterItems.Add(filterName);
                this.FilterItems.Sort();

                this.RefreshItems();
            }

            this.selectionsCount[filterName] = count + 1;
        }

        /// <summary>Unregisters a previously tracked toolbar group.</summary>
        /// <param name="filterName">Name of the toolbar group being removed.</param>
        public void RemoveSelection(string filterName)
        {
            if (!this.selectionsCount.TryGetValue(filterName, out var currentValue))
            {
                return;
            }

            currentValue--;
            if (currentValue == 0)
            {
                this.selectionsCount.Remove(filterName);
                this.FilterItems.Remove(filterName);

                this.RefreshItems();
            }
            else
            {
                this.selectionsCount[filterName] = currentValue;
            }
        }

        private void RefreshItems()
        {
            // Override the previous so that when tabs are removed it doesn't add them to hidden list thus avoiding disabling them
            this.filterValuesPrevious.Clear();

            for (var index = 0; index < this.FilterItems.Count; index++)
            {
                var filter = this.FilterItems[index];
                if (!this.SelectionsHidden.Contains(filter))
                {
                    this.filterValuesPrevious.Add(index);
                }
            }

            this.filterValues.Clear();
            this.filterValues.AddRange(this.filterValuesPrevious);

            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FilterItems)));
        }
    }
}
