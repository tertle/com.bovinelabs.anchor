namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using BovineLabs.Core.Extensions;
    using Unity.Collections;

    public struct ChangedList<T>
        where T : unmanaged
    {
        public NativeList<T> Value;
        private bool changed;

        /// <summary>
        /// Conversion marks the list as changed.
        /// </summary>
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
            this.Value.Clear();
            this.Value.AddRange(values);
            this.changed = true;
        }

        public void Add(T value)
        {
            this.Value.Add(value);
            this.changed = true;
        }

        /// <summary>
        /// Reports changes since the previous call.
        /// </summary>
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
