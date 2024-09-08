// <copyright file="LocalizationToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Localization.Settings;
    using UnityEngine.UIElements;

    [AutoToolbar("Localization")]
    public class LocalizationToolbarView : VisualElement, IView
    {
        public const string UssClassName = "bl-localization-tab";

        private readonly LocalizationToolbarViewModel viewModel = new();

        public LocalizationToolbarView()
        {
            this.AddToClassList(UssClassName);

            if (!LocalizationSettings.HasSettings)
            {
                this.Add(new Text("No LocalizationSettings"));
                return;
            }

            var dropdownField = new Dropdown
            {
                dataSource = this.viewModel,
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.viewModel.Locales[i],
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
