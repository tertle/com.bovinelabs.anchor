namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using Unity.Properties;
    using UnityEngine.Scripting;

    [Preserve]
    [IsService]
    public class ToolbarViewModel : ObservableObject
    {
        private const string SelectionKey = "bl.toolbarmanager.filter.selections";

        private readonly ILocalStorageService _storageService;
        private readonly Dictionary<string, int> _selectionsCount = new();
        private readonly HashSet<string> _selectionsHidden = new();
        private readonly List<int> _filterValuesPrevious = new();
        private readonly List<int> _filterValues = new();

        [Preserve]
        public ToolbarViewModel(ILocalStorageService storageService)
        {
            _storageService = storageService;

            var selectionSaved = storageService.GetValue(SelectionKey, string.Empty);
            var selectionArray = selectionSaved.Split(",");
            _selectionsHidden.UnionWith(selectionArray);
            _selectionsHidden.Remove(string.Empty);
        }

        public IReadOnlyCollection<string> SelectionsHidden => _selectionsHidden;

        public List<string> FilterItems { get; } = new();

        [CreateProperty]
        public IEnumerable<int> FilterValues
        {
            get => _filterValues;
            set
            {
                if (SequenceComparer.Int.Equals(_filterValues, value))
                {
                    return;
                }

                OnPropertyChanging();

                _filterValues.Clear();
                _filterValues.AddRange(value);

                foreach (var oldValue in _filterValuesPrevious.Where(oldValue => !_filterValues.Contains(oldValue)))
                {
                    _selectionsHidden.Add(FilterItems[oldValue]);
                }

                foreach (var newValue in _filterValues.Where(newValue => !_filterValuesPrevious.Contains(newValue)))
                {
                    _selectionsHidden.Remove(FilterItems[newValue]);
                }

                var serializedString = string.Join(",", _selectionsHidden);
                _storageService.SetValue(SelectionKey, serializedString);

                _filterValuesPrevious.Clear();
                _filterValuesPrevious.AddRange(_filterValues);

                OnPropertyChanged();
            }
        }

        public void AddSelection(string filterName)
        {
            _selectionsCount.TryGetValue(filterName, out var count);
            if (count == 0)
            {
                FilterItems.Add(filterName);
                FilterItems.Sort();

                RefreshItems();
            }

            _selectionsCount[filterName] = count + 1;
        }

        public void RemoveSelection(string filterName)
        {
            if (!_selectionsCount.TryGetValue(filterName, out var currentValue))
            {
                return;
            }

            currentValue--;
            if (currentValue == 0)
            {
                _selectionsCount.Remove(filterName);
                FilterItems.Remove(filterName);

                RefreshItems();
            }
            else
            {
                _selectionsCount[filterName] = currentValue;
            }
        }

        private void RefreshItems()
        {
            // Override the previous so that when tabs are removed it doesn't add them to hidden list thus avoiding disabling them
            _filterValuesPrevious.Clear();

            for (var index = 0; index < FilterItems.Count; index++)
            {
                var filter = FilterItems[index];
                if (!SelectionsHidden.Contains(filter))
                {
                    _filterValuesPrevious.Add(index);
                }
            }

            _filterValues.Clear();
            _filterValues.AddRange(_filterValuesPrevious);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(FilterItems)));
        }
    }
}
