#if BL_NERVE
namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using System.ComponentModel;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class SubSceneToolbarView : VisualElement, IDisposable
    {
        public const string UssClassName = "bl-subscene-tab";

        private readonly Dropdown _dropdown;

        [Preserve]
        public SubSceneToolbarView(SubSceneToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            _dropdown = new Dropdown
            {
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = "SubScenes",
                bindTitle = (item, _) => item.labelElement.text = "SubScenes",
                bindItem = (item, index) => Model.BindItem(item, index),
            };

            _dropdown.SetBindingTwoWay(nameof(Dropdown.value), nameof(SubSceneToolbarViewModel.SubSceneValues));

            Add(_dropdown);

            viewModel.PropertyChanged += OnPropertyChanged;
        }

        public void Dispose()
        {
            Model.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubSceneToolbarViewModel.SubScenes))
            {
                _dropdown.sourceItems = Model.SubScenes;
                _dropdown.value = Model.SubSceneValues; // Can't rely on binding to have updated in time
                _dropdown.Refresh();
            }
        }

        private SubSceneToolbarViewModel Model => (SubSceneToolbarViewModel)dataSource;
    }
}
#endif
