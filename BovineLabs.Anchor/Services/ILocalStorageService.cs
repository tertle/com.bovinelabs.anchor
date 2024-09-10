// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    public interface ILocalStorageService
    {
        string GetValue(string key, string defaultValue = default);

        T GetValue<T>(string key, T defaultValue = default);

        void SetValue(string key, string value);

        void SetValue<T>(string key, T value);

        void Delete(string key);
    }
}