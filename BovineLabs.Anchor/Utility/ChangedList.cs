// <copyright file="ChangedList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using BovineLabs.Core.Extensions;
    using Unity.Collections;

    /// <summary>
    /// Tracks whether a native list has been mutated since the last read.
    /// </summary>
    public struct ChangedList<T>
        where T : unmanaged
    {
        public NativeList<T> Value;
        private bool changed;

        /// <summary>
        /// Implicitly wraps a native list and marks it as changed.
        /// </summary>
        /// <param name="value">Native list to track.</param>
        /// <returns>A new tracker representing the supplied list.</returns>
        public static implicit operator ChangedList<T>(NativeList<T> value)
        {
            return new ChangedList<T>
            {
                Value = value,
                changed = true,
            };
        }

        /// <summary>Replaces the contents of the list and marks the value as changed.</summary>
        /// <param name="values">Values that should replace the list contents.</param>
        public void SetValue(IEnumerable<T> values)
        {
            this.Value.ClearAddRange(values);
            this.changed = true;
        }

        /// <summary>Adds a single value and marks the list as changed.</summary>
        /// <param name="value">Item to append to the list.</param>
        public void Add(T value)
        {
            this.Value.Add(value);
            this.changed = true;
        }

        /// <summary>
        /// Returns the backing list when it has changed since the last call.
        /// </summary>
        /// <param name="value">The list reference.</param>
        /// <returns>True if the list was marked as changed.</returns>
        public bool GetIfChanged(out NativeList<T> value)
        {
            value = this.Value;

            if (this.changed)
            {
                this.changed = false;
                return true;
            }

            return false;
        }
    }
}
