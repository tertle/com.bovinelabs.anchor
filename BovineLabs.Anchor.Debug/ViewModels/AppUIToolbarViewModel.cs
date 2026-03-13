// <copyright file="AppUIToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.Core;
    using Unity.Properties;

    [IsService]
    public class AppUIToolbarViewModel : ObservableObject
    {
        private const string ThemeKey = "bl.options.ui.theme";
        private const string ScaleKey = "bl.options.ui-scale";

        private readonly ILocalStorageService localStorageService;

        private readonly List<string> themes = new();
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

        [CreateProperty(ReadOnly = true)]
        public List<string> Themes { get; } = new();

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
        public List<string> Scales { get; } = new();

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
                AnchorApp.Current.Panel.Theme = Platform.darkMode ? "dark" : "light";
            }
            else
            {
                AnchorApp.Current.Panel.Theme = theme;
            }

            this.localStorageService.SetValue(ThemeKey, theme);
        }

        private void SetScale(string scale)
        {
            AnchorApp.Current.Panel.Scale = scale;
            this.localStorageService.SetValue(ScaleKey, scale);
        }

        private void OnSystemThemeChanged(bool darkMode)
        {
            AnchorApp.Current.Panel.Theme = darkMode ? "dark" : "light";
        }

        private void PopulateTheme()
        {
            this.Themes.Add("System");
            this.themes.Add("system");

            this.Themes.Add("Dark");
            this.themes.Add("dark");

            this.Themes.Add("Light");
            this.themes.Add("light");

            this.Themes.Add("Editor Dark");
            this.themes.Add("editor-dark");

            this.Themes.Add("Editor Light");
            this.themes.Add("editor-light");
        }

        private void PopulateScale()
        {
            this.Scales.Add("Small");
            this.scales.Add("small");

            this.Scales.Add("Medium");
            this.scales.Add("medium");

            this.Scales.Add("Large");
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