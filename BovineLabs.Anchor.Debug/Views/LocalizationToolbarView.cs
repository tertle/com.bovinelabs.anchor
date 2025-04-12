// <copyright file="LocalizationToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Localization.Settings;
    using UnityEngine.UIElements;

    [AutoToolbar("Localization")]
    public class LocalizationToolbarView : View<LocalizationToolbarViewModel>
    {
        public const string UssClassName = "bl-localization-tab";

        public LocalizationToolbarView()
            : base(new LocalizationToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            if (!LocalizationSettings.HasSettings)
            {
                this.Add(new Text("No LocalizationSettings"));
                return;
            }

            var dropdownField = new Dropdown
            {
                dataSource = this.ViewModel,
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.ViewModel.Locales[i],
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
    }
}
#endif
