// <copyright file="LocalizationToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_LOCALIZATION
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Unity.AppUI.MVVM;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Settings;

    [ObservableObject]
    public partial class LocalizationToolbarViewModel
    {
        [ObservableProperty]
        private int selectedLocale = -1;

        [ObservableProperty]
        private List<string> locales = new();

        public LocalizationToolbarViewModel()
        {
            if (!LocalizationSettings.HasSettings)
            {
                return;
            }

            this.PropertyChanged += this.OnPropertyChanged;

            LocalizationSettings.InitializationOperation.Completed += _ =>
            {
                this.Locales = new List<string>(LocalizationSettings.AvailableLocales.Locales.Select(s => s.ToString()));

                var locale = LocalizationSettings.SelectedLocale;
                this.SelectedLocale = locale != null ? this.Locales.IndexOf(locale.ToString()) : -1;

                LocalizationSettings.SelectedLocaleChanged += this.OnSelectedLocaleChanged;
            };
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.SelectedLocale))
            {
                LocalizationSettings.SelectedLocale = this.SelectedLocale != -1 ? LocalizationSettings.AvailableLocales.Locales[this.SelectedLocale] : null;
            }
        }

        private void OnSelectedLocaleChanged(Locale obj)
        {
            this.SelectedLocale = this.Locales.IndexOf(LocalizationSettings.SelectedLocale.ToString());
        }
    }
}
#endif
