#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using Unity.Localization;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class LocalizationToolbarView : VisualElement
    {
        public const string UssClassName = "bl-localization-tab";

        [Preserve]
        public LocalizationToolbarView(LocalizationToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            if (!LocalizationSettings.HasSettings)
            {
                Add(new Text("No LocalizationSettings"));
                return;
            }

            var dropdownField = new Dropdown
            {
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = Model.Locales[i],
            };

            dropdownField.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.Locales)),
            });

            dropdownField.SetBinding(nameof(Dropdown.selectedIndex),
                new DataBinding { dataSourcePath = new PropertyPath(nameof(LocalizationToolbarViewModel.SelectedLocale)) });

            Add(dropdownField);
        }

        private LocalizationToolbarViewModel Model => (LocalizationToolbarViewModel)dataSource;
    }
}
#endif
