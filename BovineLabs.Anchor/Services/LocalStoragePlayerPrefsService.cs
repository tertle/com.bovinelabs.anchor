// <copyright file="LocalStoragePlayerPrefsService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using UnityEngine;

    internal record LocalStoragePlayerPrefsService : ILocalStorageService
    {
        public bool HasValue(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string GetValue(string key, string defaultValue = default)
        {
            if (this.HasValue(key))
            {
                return PlayerPrefs.GetString(key);
            }

            // this.SetValue(key, defaultValue);
            return defaultValue;
        }

        public void SetValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        // public T GetValue<T>(string key, T defaultValue = default)
        // {
        //     if (PlayerPrefs.HasKey(key))
        //     {
        //         var json = PlayerPrefs.GetString(key);
        //         try
        //         {
        //             return JsonUtility.FromJson<T>(json);
        //         }
        //         catch (Exception ex)
        //         {
        //             Debug.LogException(ex);
        //         }
        //     }
        //
        //     // this.SetValue(key, defaultValue);
        //     return defaultValue;
        // }
        //
        // public void SetValue<T>(string key, T value)
        // {
        //     var json = JsonUtility.ToJson(value);
        //     PlayerPrefs.SetString(key, json);
        // }
    }
}
