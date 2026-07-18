// <copyright file="AppUIToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class AppUIToolbarView : VisualElement
    {
        public const string UssClassName = "bl-appui-tab";

        [Preserve]
        public AppUIToolbarView(AppUIToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            var theme = new Dropdown
            {
                defaultMessage = "Theme",
                bindItem = (item, i) => item.label = this.Model.Themes[i].ToString(),
            };

            theme.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.Themes)),
            });

            theme.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding { dataSourcePath = new PropertyPath(nameof(AppUIToolbarViewModel.ThemeValue)) });

            var scale = new Dropdown
            {
                defaultMessage = "Scale",
                bindItem = (item, i) => item.label = this.Model.Scales[i].ToString(),
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

        private AppUIToolbarViewModel Model => (AppUIToolbarViewModel)this.dataSource;
    }
}
