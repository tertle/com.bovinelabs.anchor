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
        private readonly List<int> filterValuesCached = new();
        private IEnumerable<int> filterValues = Array.Empty<int>();

        public ToolbarViewModel(ILocalStorageService storageService)
        {
            this.storageService = storageService;

            var selectionSaved = storageService.GetValue(SelectionKey, string.Empty);
            var selectionArray = selectionSaved.Split(",");
            this.selectionsHidden.UnionWith(selectionArray);
            this.selectionsHidden.Remove(string.Empty);
        }

        public IReadOnlyCollection<string> SelectionsHidden => this.selectionsHidden;

        [CreateProperty]
        public List<string> FilterItems => this.filterItems.ToList();

        [CreateProperty]
        public IEnumerable<int> FilterValues
        {
            get => this.filterValues;
            set
            {
                this.SetProperty(this.filterValuesCached, value, SequenceComparer.Int, value =>
                {
                    this.filterValues = value.ToArray();

                    foreach (var oldValue in this.filterValuesCached.Where(oldValue => !this.filterValues.Contains(oldValue)))
                    {
                        this.selectionsHidden.Add(this.filterItems[oldValue]);
                    }

                    foreach (var newValue in this.filterValues.Where(newValue => !this.filterValuesCached.Contains(newValue)))
                    {
                        this.selectionsHidden.Remove(this.filterItems[newValue]);
                    }

                    var serializedString = string.Join(",", this.selectionsHidden);
                    this.storageService.SetValue(SelectionKey, serializedString);

                    this.filterValuesCached.Clear();
                    this.filterValuesCached.AddRange(this.filterValues);
                });
            }
        }

        public void AddSelection(string filterName)
        {
            this.selectionsCount.TryGetValue(filterName, out var count);
            if (count == 0)
            {
                this.filterItems.Add(filterName);
                this.filterItems.Sort();
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FilterItems)));
                this.UpdateValue();
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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FilterItems)));
                this.UpdateValue();
            }
            else
            {
                this.selectionsCount[filterName] = currentValue;
            }
        }

        private void UpdateValue()
        {
            this.filterValuesCached.Clear();

            for (var index = 0; index < this.FilterItems.Count; index++)
            {
                var filter = this.FilterItems[index];
                if (!this.SelectionsHidden.Contains(filter))
                {
                    this.filterValuesCached.Add(index);
                }
            }

            this.filterValues = this.filterValuesCached.ToArray();
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.FilterValues)));
        }
    }
}
