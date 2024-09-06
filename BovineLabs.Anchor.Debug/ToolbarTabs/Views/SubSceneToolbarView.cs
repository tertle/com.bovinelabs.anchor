// <copyright file="SubSceneToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.ToolbarTabs.Views
{
    using System.ComponentModel;
    using BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public class SubSceneToolbarView : VisualElement, IView
    {
        private readonly SubSceneToolbarViewModel viewModel = new();
        private readonly Dropdown dropdown;

        public SubSceneToolbarView()
        {
            this.dropdown = new Dropdown
            {
                dataSource = this.viewModel,
                selectionType = PickerSelectionType.Multiple,
                defaultMessage = "SubScenes",
                bindTitle = (item, _) => item.labelElement.text = "SubScenes",
            };
            this.dropdown.bindItem = (item, i) => item.label = this.dropdown.sourceItems[i].ToString();

            this.dropdown.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(SubSceneToolbarViewModel.SubScenes)),
            });

            this.dropdown.SetBinding(nameof(Dropdown.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(SubSceneToolbarViewModel.SubSceneSelected)) });
            this.Add(this.dropdown);

            this.viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
        }

        object IView.ViewModel => this.viewModel;

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
