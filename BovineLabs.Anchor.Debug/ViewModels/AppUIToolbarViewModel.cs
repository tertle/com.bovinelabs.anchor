// <copyright file="AppUIToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.Core;
    using Unity.Properties;

    public class AppUIToolbarViewModel : BindableObservableObject, IViewModel
    {
        private const string ThemeKey = "bl.ui.theme";
        private const string ScaleKey = "bl.ui.scale";

        private readonly ILocalStorageService localStorageService;

        private readonly List<string> themeItems = new();
        private readonly List<string> themes = new();
        private readonly List<string> scaleItems = new();
        private readonly List<string> scales = new();

        private int themeValue = -1;
        private int scaleValue = -1;

        public AppUIToolbarViewModel(ILocalStorageService localStorageService)
        {
            this.localStorageService = localStorageService;

            this.PopulateTheme();
            this.PopulateScale();
            this.LoadStoredValue();
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
                AnchorApp.current.Panel.theme = Platform.darkMode ? "dark" : "light";
            }
            else
            {
                AnchorApp.current.Panel.theme = theme;
            }

            this.localStorageService.SetValue(ThemeKey, theme);
        }

        private void SetScale(string scale)
        {
            AnchorApp.current.Panel.scale = scale;
            this.localStorageService.SetValue(ScaleKey, scale);
        }

        private void OnSystemThemeChanged(bool darkMode)
        {
            AnchorApp.current.Panel.theme = darkMode ? "dark" : "light";
        }

        private void PopulateTheme()
        {
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
        }

        private void PopulateScale()
        {
            this.scaleItems.Add("Small");
            this.scales.Add("small");

            this.scaleItems.Add("Medium");
            this.scales.Add("medium");

            this.scaleItems.Add("Large");
            this.scales.Add("large");
        }

        private void LoadStoredValue()
        {
            var theme = this.localStorageService.GetValue(ThemeKey, "system");
            var scale = this.localStorageService.GetValue(ScaleKey, "medium");

            var themeIndex = this.themes.IndexOf(theme);
            this.ThemeValue = themeIndex != -1 ? themeIndex : 0;

            var scaleIndex = this.scales.IndexOf(scale);
            this.ScaleValue = scaleIndex != -1 ? scaleIndex : 1;
        }
    }
}
