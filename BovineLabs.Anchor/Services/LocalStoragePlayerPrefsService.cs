// <copyright file="LocalStoragePlayerPrefsService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using System.IO;
    using UnityEngine;

    internal record LocalStoragePlayerPrefsService : ILocalStorageService
    {
        private readonly string directory = Application.persistentDataPath + @"\SaveFiles\";

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

        public int GetValue(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetValue(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public bool GetValue(string key, bool defaultValue)
        {
            return this.GetValue(key, defaultValue ? 1 : 0) != 0;
        }

        public void SetValue(string key, bool value)
        {
            this.SetValue(key, value ? 1 : 0);
        }

        /// <inheritdoc />
        public bool HasJson(string key)
        {
            return this.HasKey(key);
        }

        /// <inheritdoc />
        public void DeleteJson(string key)
        {
            this.DeleteKey(key);
        }

        /// <inheritdoc />
        public T GetJson<T>(string key, T defaultValue = default)
        {
            if (this.HasJson(key))
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

        /// <inheritdoc />
        public void SetJson<T>(string key, T value)
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

        public bool HasBytes(string key)
        {
            try
            {
                return File.Exists(this.directory + key);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public void DeleteBytes(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                File.Delete(this.directory + key);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public byte[] GetBytes(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            if (!this.HasBytes(key))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(this.directory + key);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public void SetBytes(string key, byte[] value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("Setting bytes to empty file");
                return;
            }

            try
            {
                Directory.CreateDirectory(this.directory);
                File.WriteAllBytes(this.directory + key, value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
