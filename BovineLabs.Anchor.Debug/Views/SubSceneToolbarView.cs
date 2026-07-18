// <copyright file="SubSceneToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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

        private readonly Dropdown dropdown;

        [Preserve]
        public SubSceneToolbarView(SubSceneToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            this.dropdown = new Dropdown
            {
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = "SubScenes",
                bindTitle = (item, _) => item.labelElement.text = "SubScenes",
                bindItem = (item, index) => this.Model.BindItem(item, index),
            };

            this.dropdown.SetBindingTwoWay(nameof(Dropdown.value), nameof(SubSceneToolbarViewModel.SubSceneValues));

            this.Add(this.dropdown);

            viewModel.PropertyChanged += this.OnPropertyChanged;
        }

        public void Dispose()
        {
            this.Model.PropertyChanged -= this.OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubSceneToolbarViewModel.SubScenes))
            {
                this.dropdown.sourceItems = this.Model.SubScenes;
                this.dropdown.value = this.Model.SubSceneValues; // Can't rely on binding to have updated in time
                this.dropdown.Refresh();
            }
        }

        private SubSceneToolbarViewModel Model => (SubSceneToolbarViewModel)this.dataSource;
    }
}
#endif
