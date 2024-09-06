// <copyright file="LocalStoragePlayerPrefsService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using UnityEngine;

    public class LocalStoragePlayerPrefsService : ILocalStorageService
    {
        public string GetValue(string key, string defaultValue = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            this.SetValue(key, defaultValue);
            return defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (PlayerPrefs.HasKey(key))
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

            this.SetValue(key, defaultValue);
            return defaultValue;
        }

        public void SetValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public void SetValue<T>(string key, T value)
        {
            var json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, json);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}