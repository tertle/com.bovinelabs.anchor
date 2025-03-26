// <copyright file="UIArray.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections;
    using Unity.Collections;

    public class UIArray<T> : IList
        where T : unmanaged, IEquatable<T>
    {
        private NativeArray<T>.ReadOnly array;

        public UIArray(NativeArray<T>.ReadOnly array)
        {
            this.array = array;
        }

        public int Count => this.array.IsCreated ? this.array.Length : 0;

        public bool IsSynchronized => true;

        public object SyncRoot { get; } = new();

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public object this[int index]
        {
            get => this.array[index];
            set => throw new InvalidOperationException("Write not support");
        }

        public static implicit operator UIArray<T>(NativeArray<T>.ReadOnly list)
        {
            return new UIArray<T>(list);
        }

        public IEnumerator GetEnumerator()
        {
            return this.array.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Add(object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        public void Clear()
        {
            throw new InvalidOperationException("Write not support");
        }

        public bool Contains(object value)
        {
            if (value is not T t)
            {
                return false;
            }

            return this.array.Contains(t);
        }

        public int IndexOf(object value)
        {
            if (value is not T t)
            {
                return -1;
            }

            return this.array.IndexOf(t);
        }

        public void Insert(int index, object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        public void Remove(object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException("Write not support");
        }
    }
}
