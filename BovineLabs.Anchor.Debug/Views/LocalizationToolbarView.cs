// <copyright file="LocalizationToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Localization.Settings;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class LocalizationToolbarView : VisualElement
    {
        public const string UssClassName = "bl-localization-tab";

        [Preserve]
        public LocalizationToolbarView(LocalizationToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            if (!LocalizationSettings.HasSettings)
            {
                this.Add(new Text("No LocalizationSettings"));
                return;
            }

            var dropdownField = new Dropdown
            {
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.Model.Locales[i],
            };

            dropdownField.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.Locales)),
            });

            dropdownField.SetBinding(nameof(Dropdown.selectedIndex),
                new DataBinding { dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.SelectedLocale)) });

            this.Add(dropdownField);
        }

        private LocalizationToolbarViewModel Model => (LocalizationToolbarViewModel)this.dataSource;
    }
}
#endif
