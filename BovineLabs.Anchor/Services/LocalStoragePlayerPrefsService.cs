// <copyright file="LocalStoragePlayerPrefsService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine;

    internal record LocalStoragePlayerPrefsService : ILocalStorageService
    {
        /// <inheritdoc />
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <inheritdoc />
        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <inheritdoc />
        public string GetValue(string key, string defaultValue = "")
        {
            return this.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
        }

        /// <inheritdoc />
        public void SetValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        /// <inheritdoc/>
        public int GetValue(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <inheritdoc/>
        public void SetValue(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        /// <inheritdoc/>
        public bool GetValue(string key, bool defaultValue)
        {
            return this.GetValue(key, defaultValue ? 1 : 0) != 0;
        }

        /// <inheritdoc/>
        public void SetValue(string key, bool value)
        {
            this.SetValue(key, value ? 1 : 0);
        }
    }
}
