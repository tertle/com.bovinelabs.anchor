// <copyright file="ChangedList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
#if BL_CORE
    using BovineLabs.Core.Extensions;
#endif
    using Unity.Collections;

    public struct ChangedList<T>
        where T : unmanaged
    {
        public NativeList<T> Value;
        private bool changed;

        public static implicit operator ChangedList<T>(NativeList<T> value)
        {
            return new ChangedList<T>
            {
                Value = value,
                changed = true,
            };
        }

        public void SetValue(IEnumerable<T> values)
        {
#if BL_CORE
            this.Value.ClearAddRange(values);
#else
            this.Value.Clear();
            foreach (var v in values)
            {
                this.Value.Add(v);
            }
#endif
            this.changed = true;
        }

        public void Add(T value)
        {
            this.Value.Add(value);
            this.changed = true;
        }

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