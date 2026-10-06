namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    [IsService]
    public partial class CollectionSampleViewModel : ObservableObject
    {
        private int _nextId;

        [ObservableProperty]
        private CollectionItemViewModel[] _items = Array.Empty<CollectionItemViewModel>();

        [ObservableProperty]
        private CollectionItemViewModel _selected;

        [ObservableProperty]
        private int _revision;

        public CollectionSampleViewModel()
        {
            Reset();
        }

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Items))]
        public bool HasItems => Items.Length > 0;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Items))]
        public bool CanAdd => Items.Length < 36;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Items))]
        public string CountText => $"{Items.Length} relays / 36 slots";

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Selected), nameof(Revision))]
        public string SelectionText => Selected == null ? "Select a relay in either view." : $"{Selected.Title} · {Selected.Charge:P0} charge";

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Items), nameof(Revision))]
        public string TotalText
        {
            get
            {
                var total = 0f;
                foreach (var item in Items)
                {
                    total += item.Charge;
                }

                return $"{total:F2} stored charge across the collection";
            }
        }

        [ICommand(CanExecuteProperty = nameof(CanAdd))]
        private void Add()
        {
            var next = new List<CollectionItemViewModel>(Items) { CreateItem() };
            Items = next.ToArray();
        }

        [ICommand(CanExecuteProperty = nameof(HasItems))]
        private void Remove()
        {
            var next = new List<CollectionItemViewModel>(Items);
            var removed = Selected ?? next[^1];
            next.Remove(removed);
            removed.Selected = false;
            Selected = null;
            Items = next.ToArray();
        }

        [ICommand(CanExecuteProperty = nameof(HasItems))]
        private void ChargeAll()
        {
            foreach (var item in Items)
            {
                item.Charge = 1f;
            }
        }

        [ICommand(CanExecuteProperty = nameof(HasItems))]
        private void Clear()
        {
            Selected = null;
            Items = Array.Empty<CollectionItemViewModel>();
        }

        [ICommand]
        private void Reset()
        {
            _nextId = 0;
            Selected = null;
            var next = new CollectionItemViewModel[6];
            for (var index = 0; index < next.Length; index++)
            {
                next[index] = CreateItem();
            }

            Items = next;
        }

        private CollectionItemViewModel CreateItem()
        {
            var id = ++_nextId;
            return new CollectionItemViewModel(id, 0.15f + id % 5 * 0.15f, Select, OnItemChanged);
        }

        private void Select(CollectionItemViewModel item)
        {
            foreach (var entry in Items)
            {
                entry.Selected = entry == item;
            }

            Selected = item;
        }

        private void OnItemChanged()
        {
            Revision++;
        }
    }

    [Preserve]
    public partial class CollectionItemViewModel : ObservableObject
    {
        private readonly Action<CollectionItemViewModel> _onSelect;
        private readonly Action _onChanged;

        [ObservableProperty]
        [AlsoExecute(nameof(OnChargeChanged))]
        private float _charge;

        [ObservableProperty]
        private bool _selected;

        public CollectionItemViewModel(int id, float charge, Action<CollectionItemViewModel> onSelect, Action onChanged)
        {
            Title = $"Relay {id:00}";
            Description = $"Signal relay {id:00} stores charge independently. Change it here and watch the same row update in the other view.";
            _charge = charge;
            _onSelect = onSelect;
            _onChanged = onChanged;
        }

        [CreateProperty(ReadOnly = true)]
        public string Title { get; }

        [CreateProperty(ReadOnly = true)]
        public string Description { get; }

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge))]
        public string ChargeText => $"{Charge:P0} charge";

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge))]
        public bool CanCharge => Charge < 1f;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge))]
        public bool CanDischarge => Charge > 0f;

        [ICommand]
        private void Select()
        {
            _onSelect(this);
        }

        [ICommand(CanExecuteProperty = nameof(CanCharge))]
        private void ChargeUp()
        {
            Charge = Mathf.Min(1f, Charge + 0.2f);
        }

        [ICommand(CanExecuteProperty = nameof(CanDischarge))]
        private void Discharge()
        {
            Charge = Mathf.Max(0f, Charge - 0.2f);
        }

        private void OnChargeChanged()
        {
            _onChanged();
        }
    }
}
