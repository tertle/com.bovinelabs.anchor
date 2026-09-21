namespace BovineLabs.Anchor.Services
{
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    internal record LocalStoragePlayerPrefsService : ILocalStorageService
    {
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public string GetValue(string key, string defaultValue = "")
        {
            return HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
        }

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
            return GetValue(key, defaultValue ? 1 : 0) != 0;
        }

        public void SetValue(string key, bool value)
        {
            SetValue(key, value ? 1 : 0);
        }
    }
}
