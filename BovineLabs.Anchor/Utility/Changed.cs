// <copyright file="Changed.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    public struct Changed<T>
        where T : unmanaged
    {
        public T Value;
        private bool changed;

        public static implicit operator Changed<T>(T value)
        {
            return new Changed<T>
            {
                Value = value,
                changed = true,
            };
        }

        /// <summary> Gets the value, returns whether it was changed since the last time it was got and resets the change state. </summary>
        /// <param name="value"> The value. </param>
        /// <returns> True if changed, otherwise false. </returns>
        public bool Get(out T value)
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
