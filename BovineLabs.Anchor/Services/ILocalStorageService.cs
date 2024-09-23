// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    public interface ILocalStorageService
    {
        bool HasValue(string key);

        string GetValue(string key, string defaultValue = default);

        void SetValue(string key, string value);

        void Delete(string key);
    }
}
