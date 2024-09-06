// <copyright file="LocalizationToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR && UNITY_LOCALIZATION
namespace BovineLabs.Core.ToolbarTabs.Views
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BovineLabs.Core.Toolbar.App;
    using BovineLabs.Core.ToolbarTabs.ViewModels;
    using BovineLabs.Core.UI;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Settings;
    using UnityEngine.UIElements;

    [AutoToolbar("Localization")]
    public class LocalizationToolbarView : VisualElement, IView
    {
        private readonly LocalizationToolbarViewModel viewModel = new();

        public LocalizationToolbarView()
        {
            if (!LocalizationSettings.HasSettings)
            {
                this.Add(new Text("No LocalizationSettings"));
                return;
            }

            var dropdownField = new Dropdown
            {
                dataSource = viewModel,
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = viewModel.Locales[i],
            };

            dropdownField.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.Locales)),
            });

            dropdownField.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.SelectedLocale)),
            });

            this.Add(dropdownField);
        }

        object IView.ViewModel => this.viewModel;
    }
}
#endif
