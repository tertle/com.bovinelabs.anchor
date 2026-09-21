namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;

    internal sealed class TestLocalStorageService : ILocalStorageService
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public int SetStringValueCount { get; private set; }

        public string LastSetStringKey { get; private set; }

        public string LastSetStringValue { get; private set; }

        public bool HasKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            _values.Remove(key);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            return _values.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetValue(string key, string value)
        {
            var stored = value ?? string.Empty;
            _values[key] = stored;
            SetStringValueCount++;
            LastSetStringKey = key;
            LastSetStringValue = stored;
        }

        public int GetValue(string key, int defaultValue)
        {
            return _values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SetValue(string key, int value)
        {
            _values[key] = value.ToString();
        }

        public bool GetValue(string key, bool defaultValue)
        {
            return _values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SetValue(string key, bool value)
        {
            _values[key] = value.ToString();
        }
    }
}
