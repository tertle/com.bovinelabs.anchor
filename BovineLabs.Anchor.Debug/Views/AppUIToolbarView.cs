// <copyright file="AppUIToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [AutoToolbar("UI")]
    public class AppUIToolbarView : VisualElement, IView<AppUIToolbarViewModel>
    {
        public const string UssClassName = "bl-appui-tab";

        public AppUIToolbarView(AppUIToolbarViewModel viewModel)
        {
            this.AddToClassList(UssClassName);

            this.ViewModel = viewModel;

            var theme = new Dropdown
            {
                dataSource = viewModel,
                defaultMessage = "Theme",
                bindItem = (item, i) => item.label = viewModel.Themes[i].ToString(),
            };

            theme.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.Themes)),
            });

            theme.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding { dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.ThemeValue)) });

            var scale = new Dropdown
            {
                dataSource = viewModel,
                defaultMessage = "Scale",
                bindItem = (item, i) => item.label = viewModel.Scales[i].ToString(),
            };

            scale.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.Scales)),
            });

            scale.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding { dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.ScaleValue)) });

            this.Add(new Text("Theme"));
            this.Add(theme);
            this.Add(new Text("Scale"));
            this.Add(scale);
        }

        public AppUIToolbarViewModel ViewModel { get; }
    }
}
