// <copyright file="UIArray.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections;
    using Unity.Collections;

    /// <summary>
    /// Read-only <see cref="IList"/> wrapper over an unmanaged native array so it can be bound in UITK.
    /// </summary>
    public class UIArray<T> : IList
        where T : unmanaged, IEquatable<T>
    {
        private NativeArray<T>.ReadOnly array;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIArray{T}"/> class.
        /// </summary>
        public UIArray(MultiContainer<T> array)
        {
            this.array = array.AsArray();
        }

        /// <summary>Gets the number of elements in the array.</summary>
        public int Count => this.array.IsCreated ? this.array.Length : 0;

        /// <summary>Gets a value indicating whether access to the collection is thread-safe.</summary>
        public bool IsSynchronized => true;

        /// <summary>Gets an object that can be used to synchronize access.</summary>
        public object SyncRoot { get; } = new();

        /// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
        public bool IsFixedSize => true;

        /// <summary>Gets a value indicating whether the collection is read-only.</summary>
        public bool IsReadOnly => true;

        /// <summary>Gets or throws when attempting to set the element at the specified index.</summary>
        public object this[int index]
        {
            get => this.array[index];
            set => throw new InvalidOperationException("Write not supported");
        }

        /// <summary>Creates a read-only UIArray from the supplied multi-container.</summary>
        public static implicit operator UIArray<T>(MultiContainer<T> list)
        {
            return new UIArray<T>(list);
        }

        /// <summary>Gets an enumerator that iterates through the collection.</summary>
        public IEnumerator GetEnumerator()
        {
            return this.array.GetEnumerator();
        }

        /// <summary>Copy operation is not supported because the collection is read-only.</summary>
        public void CopyTo(Array a, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>Adding is not supported.</summary>
        public int Add(object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Clearing is not supported.</summary>
        public void Clear()
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Determines whether the collection contains the provided value.</summary>
        public bool Contains(object value)
        {
            if (value is not T t)
            {
                return false;
            }

            return this.array.Contains(t);
        }

        /// <summary>Gets the index of the provided value or -1 if the value is not present.</summary>
        public int IndexOf(object value)
        {
            if (value is not T t)
            {
                return -1;
            }

            return this.array.IndexOf(t);
        }

        /// <summary>Insertion is not supported.</summary>
        public void Insert(int index, object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Removal is not supported.</summary>
        public void Remove(object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Removal is not supported.</summary>
        public void RemoveAt(int index)
        {
            throw new InvalidOperationException("Write not support");
        }
    }
}
