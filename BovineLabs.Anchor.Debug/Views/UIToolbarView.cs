namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    [Preserve]
    public class UIToolbarView : VisualElement
    {
        public const string UssClassName = "bl-ui-tab";

        [Preserve]
        public UIToolbarView(UIToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            var theme = new Dropdown
            {
                defaultMessage = "Theme",
                bindItem = (item, i) => item.label = Model.Themes[i].ToString(),
            };

            theme.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(UIToolbarViewModel.Themes)),
            });

            theme.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding { dataSourcePath = new PropertyPath(nameof(UIToolbarViewModel.ThemeValue)) });

            var scale = new Dropdown
            {
                defaultMessage = "Scale",
                bindItem = (item, i) => item.label = Model.Scales[i].ToString(),
            };

            scale.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(UIToolbarViewModel.Scales)),
            });

            scale.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding { dataSourcePath = new PropertyPath(nameof(UIToolbarViewModel.ScaleValue)) });

            var gameUI = new Toggle();
            gameUI.SetBinding(nameof(Toggle.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(UIToolbarViewModel.GameUIVisible)) });

            Add(new ToolbarRow("Game UI", gameUI));
            Add(new ToolbarRow("Theme", theme));
            Add(new ToolbarRow("Scale", scale));
        }

        private UIToolbarViewModel Model => (UIToolbarViewModel)dataSource;
    }
}
