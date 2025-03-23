// <copyright file="UIList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections;
    using BovineLabs.Core.Extensions;
    using Unity.Collections;

    public class UIList<T> : IList
        where T : unmanaged, IEquatable<T>
    {
        private NativeList<T> list;

        public UIList(NativeList<T> list)
        {
            this.list = list;
        }

        public int Count => this.list.IsCreated ? this.list.Length : 0;

        public bool IsSynchronized => false;

        public object SyncRoot { get; } = new();

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public object this[int index]
        {
            get => this.list[index];
            set
            {
                if (value is not T t)
                {
                    return;
                }

                this.list[index] = t;
            }
        }

        public static implicit operator UIList<T>(NativeList<T> list)
        {
            return new UIList<T>(list);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.AsArray().GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Add(object value)
        {
            if (value is not T t)
            {
                return -1;
            }

            this.list.Add(t);
            return this.list.Length - 1;
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(object value)
        {
            if (value is not T t)
            {
                return false;
            }

            return this.list.Contains(t);
        }

        public int IndexOf(object value)
        {
            if (value is not T t)
            {
                return -1;
            }

            return this.list.IndexOf(t);
        }

        public void Insert(int index, object value)
        {
            if (value is not T t)
            {
                return;
            }

            this.list.Insert(index, t);
        }

        public void Remove(object value)
        {
            var index = this.IndexOf(value);
            if (index == -1)
            {
                return;
            }

            this.list.RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }
    }
}
