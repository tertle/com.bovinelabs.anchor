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
    public class LocalizationToolbarViewModel : ObservableObject, IToolbarElement, ILoadable
    {
        private int selectedLocale = -1;

        private List<string> locales = new();

        private bool loaded;

        [CreateProperty]
        public int SelectedLocale
        {
            get => this.selectedLocale;
            set
            {
                if (this.SetProperty(ref this.selectedLocale, value) && LocalizationSettings.HasSettings)
                {
                    LocalizationSettings.SelectedLocale = this.SelectedLocale != -1
                        ? LocalizationSettings.Instance.AvailableLocales[this.SelectedLocale]
                        : null;
                }
            }
        }

        [CreateProperty]
        public List<string> Locales
        {
            get => this.locales;
            set => this.SetProperty(ref this.locales, value);
        }

        public VisualElement CreateElement()
        {
            return new LocalizationToolbarView(this);
        }

        public void Load()
        {
            if (this.loaded || !LocalizationSettings.HasSettings)
            {
                return;
            }

            this.loaded = true;
            LocalizationSettings.InitializationCompleted += this.OnInitializationCompleted;
            if (LocalizationSettings.Instance.IsInitialized)
            {
                this.OnInitializationCompleted();
            }
        }

        public void Unload()
        {
            if (!this.loaded)
            {
                return;
            }

            this.loaded = false;
            LocalizationSettings.InitializationCompleted -= this.OnInitializationCompleted;
            LocalizationSettings.SelectedLocaleChanged -= this.OnSelectedLocaleChanged;
        }

        private void OnInitializationCompleted()
        {
            if (!this.loaded)
            {
                return;
            }

            this.Locales = new List<string>(LocalizationSettings.Instance.AvailableLocales.Select(s => s.ToString()));

            var locale = LocalizationSettings.SelectedLocale;
            this.SelectedLocale = locale != null ? this.Locales.IndexOf(locale.ToString()) : -1;

            LocalizationSettings.SelectedLocaleChanged -= this.OnSelectedLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += this.OnSelectedLocaleChanged;
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            this.SelectedLocale = locale != null ? this.Locales.IndexOf(locale.ToString()) : -1;
        }
    }
}
#endif

