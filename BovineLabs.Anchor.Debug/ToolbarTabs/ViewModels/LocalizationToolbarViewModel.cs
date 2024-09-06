// <copyright file="LocalizationToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR && UNITY_LOCALIZATION
namespace BovineLabs.Core.ToolbarTabs.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core.UI;
    using Unity.Properties;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Settings;

    public class LocalizationToolbarViewModel : BLObservableObject
    {
        private int selectedLocale = -1;
        private List<string> locales = new();

        public LocalizationToolbarViewModel()
        {
            if (!LocalizationSettings.HasSettings)
            {
                return;
            }

            LocalizationSettings.InitializationOperation.Completed += _ =>
            {
                this.Locales = new List<string>(LocalizationSettings.AvailableLocales.Locales.Select(s => s.ToString()));

                var locale = LocalizationSettings.SelectedLocale;
                this.SelectedLocale = locale != null ? this.Locales.IndexOf(locale.ToString()) : -1;

                LocalizationSettings.SelectedLocaleChanged += this.OnSelectedLocaleChanged;
            };
        }

        [CreateProperty]
        public List<string> Locales
        {
            get => this.locales;
            set => this.SetProperty(ref this.locales, value);
        }

        [CreateProperty]
        public int SelectedLocale
        {
            get => this.selectedLocale;
            set
            {
                if (this.SetProperty(ref this.selectedLocale, value))
                {
                    LocalizationSettings.SelectedLocale = value != -1 ? LocalizationSettings.AvailableLocales.Locales[value] : default;
                }
            }
        }

        private void OnSelectedLocaleChanged(Locale obj)
        {
            this.SelectedLocale = this.Locales.IndexOf(LocalizationSettings.SelectedLocale.ToString());
        }
    }
}
#endif
