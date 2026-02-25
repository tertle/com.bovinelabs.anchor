// <copyright file="TestLocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;

    internal sealed class TestLocalStorageService : ILocalStorageService
    {
        private readonly Dictionary<string, string> values = new(StringComparer.Ordinal);

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
    }
}
