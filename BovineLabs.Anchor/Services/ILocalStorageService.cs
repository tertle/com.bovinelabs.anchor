// <copyright file="ILocalStorageService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    /// <summary>
    /// Abstraction over player storage that supports primitive values, JSON payloads, and raw bytes.
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

        /// <summary>Determines whether a JSON payload exists for the key.</summary>
        /// <param name="key">Key to query.</param>
        /// <returns><c>true</c> if a payload exists.</returns>
        bool HasJson(string key);

        /// <summary>Removes the JSON payload associated with the key.</summary>
        /// <param name="key">Key to delete.</param>
        void DeleteJson(string key);

        /// <summary>Deserializes a JSON payload stored for the key.</summary>
        /// <typeparam name="T">Type that will be deserialized.</typeparam>
        /// <param name="key">Key to query.</param>
        /// <param name="defaultValue">Value returned when the key is missing.</param>
        /// <returns>The deserialized payload or the default.</returns>
        T GetJson<T>(string key, T defaultValue);

        /// <summary>Serializes and stores a value as JSON.</summary>
        /// <typeparam name="T">Type being serialized.</typeparam>
        /// <param name="key">Key to write.</param>
        /// <param name="value">Value to serialize.</param>
        void SetJson<T>(string key, T value);

        /// <summary>Determines whether raw bytes exist for the key.</summary>
        /// <param name="key">Key to query.</param>
        /// <returns><c>true</c> if bytes are stored.</returns>
        bool HasBytes(string key);

        /// <summary>Deletes the bytes stored for the key.</summary>
        /// <param name="key">Key to delete.</param>
        void DeleteBytes(string key);

        /// <summary>Reads the bytes stored for the key.</summary>
        /// <param name="key">Key to query.</param>
        /// <returns>The stored bytes.</returns>
        byte[] GetBytes(string key);

        /// <summary>Writes a byte array to storage.</summary>
        /// <param name="key">Key to write.</param>
        /// <param name="value">Bytes to persist.</param>
        void SetBytes(string key, byte[] value);
    }
}
