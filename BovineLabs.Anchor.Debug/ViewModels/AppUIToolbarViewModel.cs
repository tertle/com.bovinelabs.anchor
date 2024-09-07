// <copyright file="AppUIToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;

    public class AppUIToolbarViewModel : BLObservableObject, IViewModel
    {
        private readonly Panel panel;

        private readonly List<string> themeItems = new();
        private readonly List<string> themes = new();
        private readonly List<string> scaleItems = new();
        private readonly List<string> scales = new();

        private int themeValue;
        private int scaleValue;

        public AppUIToolbarViewModel(Panel panel)
        {
            this.panel = panel;
            this.themeItems.Add("System");
            this.themes.Add("system");

            this.themeItems.Add("Dark");
            this.themes.Add("dark");

            this.themeItems.Add("Light");
            this.themes.Add("light");

            this.themeItems.Add("Editor Dark");
            this.themes.Add("editor-dark");

            this.themeItems.Add("Editor Light");
            this.themes.Add("editor-light");

            this.scaleItems.Add("Small");
            this.scales.Add("small");

            this.scaleItems.Add("Medium");
            this.scales.Add("medium");

            this.scaleItems.Add("Large");
            this.scales.Add("large");

            this.scaleValue = 1;

            // TODO needed?
            this.SetTheme("system");
            this.SetScale("medium");
        }

        [CreateProperty]
        public List<string> Themes => this.themeItems;

        [CreateProperty]
        public int ThemeValue
        {
            get => this.themeValue;
            set
            {
                if (this.SetProperty(ref this.themeValue, value))
                {
                    this.SetTheme(this.themes[this.themeValue]);
                }
            }
        }

        [CreateProperty]
        public List<string> Scales => this.scaleItems;

        [CreateProperty]
        public int ScaleValue
        {
            get => this.scaleValue;
            set
            {
                if (this.SetProperty(ref this.scaleValue, value))
                {
                    this.SetScale(this.scales[this.scaleValue]);
                }
            }
        }

        private void SetTheme(string theme)
        {
            Platform.darkModeChanged -= this.OnSystemThemeChanged;
            if (theme == "system")
            {
                Platform.darkModeChanged += this.OnSystemThemeChanged;
                this.panel.theme = Platform.darkMode ? "dark" : "light";
            }
            else
            {
                this.panel.theme = theme;
            }
        }

        private void SetScale(string scale)
        {
            this.panel.scale = scale;
        }

        private void OnSystemThemeChanged(bool darkMode)
        {
            this.panel.theme = darkMode ? "dark" : "light";
        }
    }
}
