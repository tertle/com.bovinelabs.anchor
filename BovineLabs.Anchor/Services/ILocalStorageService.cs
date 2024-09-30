// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    public interface ILocalStorageService
    {
        bool HasKey(string key);

        string GetValue(string key, string defaultValue = default);

        void SetValue(string key, string value);

        void DeleteKey(string key);

        T GetValue<T>(string key, T defaultValue);

        void SetValue<T>(string key, T value);
    }
}
