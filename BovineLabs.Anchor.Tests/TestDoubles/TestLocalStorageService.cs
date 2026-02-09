// <copyright file="TestLocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;
    using UnityEngine;

    internal sealed class TestLocalStorageService : ILocalStorageService
    {
        private readonly Dictionary<string, string> values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> jsonValues = new(StringComparer.Ordinal);
        private readonly Dictionary<string, byte[]> bytes = new(StringComparer.Ordinal);

        public int SetStringValueCount { get; private set; }

        public string LastSetStringKey { get; private set; }

        public string LastSetStringValue { get; private set; }

        public bool HasKey(string key)
        {
            return this.values.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            this.values.Remove(key);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            return this.values.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetValue(string key, string value)
        {
            var stored = value ?? string.Empty;
            this.values[key] = stored;
            this.SetStringValueCount++;
            this.LastSetStringKey = key;
            this.LastSetStringValue = stored;
        }

        public int GetValue(string key, int defaultValue)
        {
            return this.values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SetValue(string key, int value)
        {
            this.values[key] = value.ToString();
        }

        public bool GetValue(string key, bool defaultValue)
        {
            return this.values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SetValue(string key, bool value)
        {
            this.values[key] = value.ToString();
        }

        public bool HasJson(string key)
        {
            return this.jsonValues.ContainsKey(key);
        }

        public void DeleteJson(string key)
        {
            this.jsonValues.Remove(key);
        }

        public T GetJson<T>(string key, T defaultValue)
        {
            if (!this.jsonValues.TryGetValue(key, out var raw))
            {
                return defaultValue;
            }

            try
            {
                return JsonUtility.FromJson<T>(raw);
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetJson<T>(string key, T value)
        {
            this.jsonValues[key] = JsonUtility.ToJson(value);
        }

        public bool HasBytes(string key)
        {
            return this.bytes.ContainsKey(key);
        }

        public void DeleteBytes(string key)
        {
            this.bytes.Remove(key);
        }

        public byte[] GetBytes(string key)
        {
            return this.bytes.TryGetValue(key, out var value) ? value : null;
        }

        public void SetBytes(string key, byte[] value)
        {
            this.bytes[key] = value;
        }
    }
}
