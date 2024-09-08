// <copyright file="ToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Unity.Properties;

    public class ToolbarViewModel : BLObservableObject, IViewModel
    {
        private const string SelectionKey = "bl.toolbarmanager.filter.selections";

        private readonly ILocalStorageService storageService;
        private readonly Dictionary<string, int> selectionsCount = new();
        private readonly HashSet<string> selectionsHidden = new();
        private readonly List<string> filterItems = new();
        private readonly List<int> filterValuesPrevious = new();
        private readonly List<int> filterValues = new();

        public ToolbarViewModel(ILocalStorageService storageService)
        {
            this.storageService = storageService;

            var selectionSaved = storageService.GetValue(SelectionKey, string.Empty);
            var selectionArray = selectionSaved.Split(",");
            this.selectionsHidden.UnionWith(selectionArray);
            this.selectionsHidden.Remove(string.Empty);
        }

        public IReadOnlyCollection<string> SelectionsHidden => this.selectionsHidden;

        public List<string> FilterItems => this.filterItems;

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
                    this.selectionsHidden.Add(this.filterItems[oldValue]);
                }

                foreach (var newValue in this.filterValues.Where(newValue => !this.filterValuesPrevious.Contains(newValue)))
                {
                    this.selectionsHidden.Remove(this.filterItems[newValue]);
                }

                var serializedString = string.Join(",", this.selectionsHidden);
                this.storageService.SetValue(SelectionKey, serializedString);

                this.filterValuesPrevious.Clear();
                this.filterValuesPrevious.AddRange(this.filterValues);

                this.OnPropertyChanged();
            }
        }

        public void AddSelection(string filterName)
        {
            this.selectionsCount.TryGetValue(filterName, out var count);
            if (count == 0)
            {
                this.filterItems.Add(filterName);
                this.filterItems.Sort();

                this.RefreshItems();
            }

            this.selectionsCount[filterName] = count + 1;
        }

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
                this.filterItems.Remove(filterName);

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
