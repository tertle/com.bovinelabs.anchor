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
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Transient]
    [Preserve]
    public class SubSceneToolbarView : View<SubSceneToolbarViewModel>, IDisposable
    {
        public const string UssClassName = "bl-subscene-tab";

        private readonly Dropdown dropdown;

        public SubSceneToolbarView()
            : base(new SubSceneToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            this.dropdown = new Dropdown
            {
                dataSource = this.ViewModel,
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = "SubScenes",
                bindTitle = (item, _) => item.labelElement.text = "SubScenes",
                bindItem = this.ViewModel.BindItem,
            };

            this.dropdown.SetBindingTwoWay(nameof(Dropdown.value), nameof(SubSceneToolbarViewModel.SubSceneValues));

            this.Add(this.dropdown);

            this.ViewModel.PropertyChanged += this.OnPropertyChanged;
        }

        public void Dispose()
        {
            this.ViewModel.PropertyChanged -= this.OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubSceneToolbarViewModel.SubScenes))
            {
                this.dropdown.sourceItems = this.ViewModel.SubScenes;
                this.dropdown.value = this.ViewModel.SubSceneValues; // Can't rely on binding to have updated in time
                this.dropdown.Refresh();
            }
        }
    }
}
#endif
