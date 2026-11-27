#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using Unity.Localization;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Localization")]
    public partial class LocalizationToolbarViewModel : ObservableObject, IToolbarElement, ILoadable
    {
        private bool _loaded;

        [CreateProperty]
        public int SelectedLocale
        {
            get;
            set
            {
                if (SetProperty(ref field, value) && LocalizationSettings.HasSettings)
                {
                    LocalizationSettings.SelectedLocale = SelectedLocale != -1
                        ? LocalizationSettings.Instance.AvailableLocales[SelectedLocale]
                        : null;
                }
            }
        } = -1;

        [ObservableProperty]
        public partial List<string> Locales
        {
            get;
            set;
        } = new();

        public VisualElement CreateElement()
        {
            return new LocalizationToolbarView(this);
        }

        public void Load()
        {
            if (_loaded || !LocalizationSettings.HasSettings)
            {
                return;
            }

            _loaded = true;
            LocalizationSettings.InitializationCompleted += OnInitializationCompleted;
            if (LocalizationSettings.Instance.IsInitialized)
            {
                OnInitializationCompleted();
            }
        }

        public void Unload()
        {
            if (!_loaded)
            {
                return;
            }

            _loaded = false;
            LocalizationSettings.InitializationCompleted -= OnInitializationCompleted;
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void OnInitializationCompleted()
        {
            if (!_loaded)
            {
                return;
            }

            Locales = new List<string>(LocalizationSettings.Instance.AvailableLocales.Select(s => s.LocaleName));

            var locale = LocalizationSettings.SelectedLocale;
            SelectedLocale = locale != null ? LocalizationSettings.Instance.AvailableLocales.ToList().IndexOf(locale) : -1;

            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            SelectedLocale = locale != null ? LocalizationSettings.Instance.AvailableLocales.ToList().IndexOf(locale) : -1;
        }
    }
}
#endif

