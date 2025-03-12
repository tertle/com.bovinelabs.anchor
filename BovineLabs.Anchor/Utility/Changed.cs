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

        public bool GetIfChanged(out T value)
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
