// <copyright file="LocalStoragePlayerPrefsService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using UnityEngine;

    internal record LocalStoragePlayerPrefsService : ILocalStorageService
    {
        /// <inheritdoc/>
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <inheritdoc/>
        public string GetValue(string key, string defaultValue = default)
        {
            return this.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
        }

        /// <inheritdoc/>
        public void SetValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        /// <inheritdoc/>
        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <inheritdoc/>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (this.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                try
                {
                    return JsonUtility.FromJson<T>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            return defaultValue;
        }

        /// <inheritdoc/>
        public void SetValue<T>(string key, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonUtility.ToJson(value);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exception($"Unable to serialize type {typeof(T)}");
            }

            PlayerPrefs.SetString(key, json);
        }
    }
}
