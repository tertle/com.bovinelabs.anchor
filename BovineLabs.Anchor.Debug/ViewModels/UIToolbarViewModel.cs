namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.Core;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("UI")]
    public class UIToolbarViewModel : ObservableObject, IToolbarElement, ILoadable
    {
        private const string ThemeKey = "bl.options.ui.theme";
        private const string ScaleKey = "bl.options.ui-scale";

        private readonly ILocalStorageService _localStorageService;

        private readonly List<string> _themes = new();
        private readonly List<string> _scales = new();

        private AnchorApp _app;
        private bool _loaded;
        private int _themeValue = -1;
        private int _scaleValue = -1;

        [Preserve]
        public UIToolbarViewModel(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;

            PopulateTheme();
            PopulateScale();
            LoadStoredValue();
        }

        [CreateProperty(ReadOnly = true)]
        public List<string> Themes { get; } = new();

        [CreateProperty]
        public int ThemeValue
        {
            get => _themeValue;
            set
            {
                if (SetProperty(ref _themeValue, value))
                {
                    SetTheme(_themes[_themeValue]);
                }
            }
        }

        [CreateProperty]
        public List<string> Scales { get; } = new();

        [CreateProperty]
        public int ScaleValue
        {
            get => _scaleValue;
            set
            {
                if (SetProperty(ref _scaleValue, value))
                {
                    SetScale(_scales[_scaleValue]);
                }
            }
        }

        [CreateProperty]
        public bool GameUIVisible
        {
            get => _app.UIVisible;
            set => _app.UIVisible = value;
        }

        public VisualElement CreateElement()
        {
            return new UIToolbarView(this);
        }

        public void Load()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            RefreshSystemThemeSubscription();

            _app = AnchorApp.Current;
            _app.UIVisibilityChanged += OnUIVisibilityChanged;
        }

        public void Unload()
        {
            _loaded = false;
            Platform.darkModeChanged -= OnSystemThemeChanged;

            _app.UIVisibilityChanged -= OnUIVisibilityChanged;
            _app = null;
        }

        private void OnUIVisibilityChanged(bool visible)
        {
            OnPropertyChanged(nameof(GameUIVisible));
        }

        private void SetTheme(string theme)
        {
            Platform.darkModeChanged -= OnSystemThemeChanged;
            if (theme == "system")
            {
                if (_loaded)
                {
                    Platform.darkModeChanged += OnSystemThemeChanged;
                }

                AnchorApp.Current.Theme = Platform.darkMode ? "dark" : "light";
            }
            else
            {
                AnchorApp.Current.Theme = theme;
            }

            _localStorageService.SetValue(ThemeKey, theme);
        }

        private void RefreshSystemThemeSubscription()
        {
            Platform.darkModeChanged -= OnSystemThemeChanged;
            if (_themeValue != -1 && _themes[_themeValue] == "system")
            {
                Platform.darkModeChanged += OnSystemThemeChanged;
            }
        }

        private void SetScale(string scale)
        {
            AnchorApp.Current.Scale = scale;
            _localStorageService.SetValue(ScaleKey, scale);
        }

        private void OnSystemThemeChanged(bool darkMode)
        {
            AnchorApp.Current.Theme = darkMode ? "dark" : "light";
        }

        private void PopulateTheme()
        {
            Themes.Add("System");
            _themes.Add("system");

            Themes.Add("Dark");
            _themes.Add("dark");

            Themes.Add("Light");
            _themes.Add("light");

            Themes.Add("Editor Dark");
            _themes.Add("editor-dark");

            Themes.Add("Editor Light");
            _themes.Add("editor-light");
        }

        private void PopulateScale()
        {
            Scales.Add("Small");
            _scales.Add("small");

            Scales.Add("Medium");
            _scales.Add("medium");

            Scales.Add("Large");
            _scales.Add("large");
        }

        private void LoadStoredValue()
        {
            var theme = _localStorageService.GetValue(ThemeKey, "system");
            var scale = _localStorageService.GetValue(ScaleKey, "medium");

            var themeIndex = _themes.IndexOf(theme);
            ThemeValue = themeIndex != -1 ? themeIndex : 0;

            var scaleIndex = _scales.IndexOf(scale);
            ScaleValue = scaleIndex != -1 ? scaleIndex : 1;
        }
    }
}
