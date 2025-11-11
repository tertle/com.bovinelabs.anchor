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
        bool HasKey(string key);

        /// <summary>Removes the stored value associated with the provided key.</summary>
        void DeleteKey(string key);

        /// <summary>Gets the stored string value or returns the provided default.</summary>
        string GetValue(string key, string defaultValue = null);

        /// <summary>Stores a string value.</summary>
        void SetValue(string key, string value);

        /// <summary>Gets an integer value or a default when the key is missing.</summary>
        int GetValue(string key, int defaultValue);

        /// <summary>Stores an integer value.</summary>
        void SetValue(string key, int value);

        /// <summary>Gets a boolean value or a default when the key is missing.</summary>
        bool GetValue(string key, bool defaultValue);

        /// <summary>Stores a boolean value.</summary>
        void SetValue(string key, bool value);

        /// <summary>Determines whether a JSON payload exists for the key.</summary>
        bool HasJson(string key);

        /// <summary>Removes the JSON payload associated with the key.</summary>
        void DeleteJson(string key);

        /// <summary>Deserializes a JSON payload stored for the key.</summary>
        T GetJson<T>(string key, T defaultValue);

        /// <summary>Serializes and stores a value as JSON.</summary>
        void SetJson<T>(string key, T value);

        /// <summary>Determines whether raw bytes exist for the key.</summary>
        bool HasBytes(string key);

        /// <summary>Deletes the bytes stored for the key.</summary>
        void DeleteBytes(string key);

        /// <summary>Reads the bytes stored for the key.</summary>
        byte[] GetBytes(string key);

        /// <summary>Writes a byte array to storage.</summary>
        void SetBytes(string key, byte[] value);
    }
}
