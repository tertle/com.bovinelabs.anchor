// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    public interface ILocalStorageService
    {
        bool HasKey(string key);

        void DeleteKey(string key);

        string GetValue(string key, string defaultValue = null);

        void SetValue(string key, string value);

        bool HasJson(string key);

        void DeleteJson(string key);

        T GetJson<T>(string key, T defaultValue);

        void SetJson<T>(string key, T value);

        bool HasBytes(string key);

        void DeleteBytes(string key);

        byte[] GetBytes(string key);

        void SetBytes(string key, byte[] value);
    }
}
