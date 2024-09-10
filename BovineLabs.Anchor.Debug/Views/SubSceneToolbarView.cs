// <copyright file="SubSceneToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using System.ComponentModel;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [Transient]
    public class SubSceneToolbarView : VisualElement, IView, IDisposable
    {
        public const string UssClassName = "bl-subscene-tab";

        private readonly SubSceneToolbarViewModel viewModel = new();
        private readonly Dropdown dropdown;

        public SubSceneToolbarView()
        {
            this.AddToClassList(UssClassName);

            this.dropdown = new Dropdown
            {
                dataSource = this.viewModel,
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = "SubScenes",
                bindTitle = (item, _) => item.labelElement.text = "SubScenes",
            };

            this.dropdown.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(SubSceneToolbarViewModel.SubScenes)),
            });

            this.dropdown.SetBinding(
                nameof(Dropdown.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(SubSceneToolbarViewModel.SubSceneValues)) });

            this.dropdown.SetBinding(
                nameof(Dropdown.bindItem), new DataBinding { dataSourcePath = new PropertyPath(nameof(SubSceneToolbarViewModel.BindItem)) });

            this.Add(this.dropdown);

            this.viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
        }

        object IView.ViewModel => this.viewModel;

        public void Dispose()
        {
            this.viewModel.PropertyChanged -= this.ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubSceneToolbarViewModel.SubScenes))
            {
                this.dropdown.Refresh();
            }
        }
    }
}
#endif
