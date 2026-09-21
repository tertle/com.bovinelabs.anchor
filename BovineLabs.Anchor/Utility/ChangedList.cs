namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using BovineLabs.Core.Extensions;
    using Unity.Collections;

    public struct ChangedList<T>
        where T : unmanaged
    {
        public NativeList<T> Value;
        private bool _changed;

        /// <summary>
        /// Conversion marks the list as changed.
        /// </summary>
        public static implicit operator ChangedList<T>(NativeList<T> value)
        {
            return new ChangedList<T>
            {
                Value = value,
                _changed = true,
            };
        }

        public void SetValue(IEnumerable<T> values)
        {
            Value.Clear();
            Value.AddRange(values);
            _changed = true;
        }

        public void Add(T value)
        {
            Value.Add(value);
            _changed = true;
        }

        /// <summary>
        /// Reports changes since the previous call.
        /// </summary>
        public bool GetIfChanged(out NativeList<T> value)
        {
            value = Value;

            if (_changed)
            {
                _changed = false;
                return true;
            }

            return false;
        }
    }
}
