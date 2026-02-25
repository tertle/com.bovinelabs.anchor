// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    /// <summary>
    /// Abstraction over player storage for primitive key/value preferences.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>Determines whether a string value exists for the provided key.</summary>
        /// <param name="key">Key to query.</param>
        /// <returns><c>true</c> if the key is present.</returns>
        bool HasKey(string key);

        /// <summary>Removes the stored value associated with the provided key.</summary>
        /// <param name="key">Key to delete.</param>
        void DeleteKey(string key);

        /// <summary>Gets the stored string value or returns the provided default.</summary>
        /// <param name="key">Key to query.</param>
        /// <param name="defaultValue">Value returned when the key is missing.</param>
        /// <returns>The stored string or the default.</returns>
        string GetValue(string key, string defaultValue = null);

        /// <summary>Stores a string value.</summary>
        /// <param name="key">Key to write.</param>
        /// <param name="value">Value to persist.</param>
        void SetValue(string key, string value);

        /// <summary>Gets an integer value or a default when the key is missing.</summary>
        /// <param name="key">Key to query.</param>
        /// <param name="defaultValue">Value returned when the key is missing.</param>
        /// <returns>The stored integer or the default.</returns>
        int GetValue(string key, int defaultValue);

        /// <summary>Stores an integer value.</summary>
        /// <param name="key">Key to write.</param>
        /// <param name="value">Value to persist.</param>
        void SetValue(string key, int value);

        /// <summary>Gets a boolean value or a default when the key is missing.</summary>
        /// <param name="key">Key to query.</param>
        /// <param name="defaultValue">Value returned when the key is missing.</param>
        /// <returns>The stored boolean or the default.</returns>
        bool GetValue(string key, bool defaultValue);

        /// <summary>Stores a boolean value.</summary>
        /// <param name="key">Key to write.</param>
        /// <param name="value">Value to persist.</param>
        void SetValue(string key, bool value);
    }
}
