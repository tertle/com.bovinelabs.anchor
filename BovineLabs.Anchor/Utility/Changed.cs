// <copyright file="Changed.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    [Serializable]
    /// <summary>
    /// Lightweight wrapper that records whether a value changed since the last update.
    /// </summary>
    public readonly struct Changed<T>
        where T : unmanaged
    {
        /// <summary>Gets the wrapped value.</summary>
        public readonly T Value;

        // Used by source generators
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        /// <summary>Gets a value indicating whether the value changed.</summary>
        public readonly bool HasChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Changed{T}"/> struct.
        /// </summary>
        /// <param name="value">Value to wrap.</param>
        /// <param name="hasChanged">Whether the value is marked as changed.</param>
        public Changed(T value, bool hasChanged = true)
        {
            this.Value = value;
            this.HasChanged = hasChanged;
        }

        /// <summary>Wraps a value and marks it as changed.</summary>
        /// <param name="value">Value to wrap.</param>
        /// <returns>A changed wrapper representing the supplied value.</returns>
        public static implicit operator Changed<T>(T value)
        {
            return new Changed<T>(value);
        }
    }
}
