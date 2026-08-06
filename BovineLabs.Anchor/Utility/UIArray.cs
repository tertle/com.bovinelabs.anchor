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
        /// <param name="array">Container that exposes the underlying native array.</param>
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
        /// <param name="list">Container that exposes the underlying native array.</param>
        /// <returns>A read-only wrapper over the provided container.</returns>
        public static implicit operator UIArray<T>(MultiContainer<T> list)
        {
            return new UIArray<T>(list);
        }

        /// <summary>Gets an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that iterates through the array.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.array.GetEnumerator();
        }

        /// <summary>Copies the collection into a compatible one-dimensional array.</summary>
        /// <param name="array">Destination array.</param>
        /// <param name="index">Zero-based destination index at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1 || array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("The destination must be a zero-based, one-dimensional array.", nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index > array.Length || array.Length - index < this.Count)
            {
                throw new ArgumentException("The destination array does not have enough available space.", nameof(array));
            }

            try
            {
                for (var i = 0; i < this.Count; i++)
                {
                    array.SetValue(this.array[i], index + i);
                }
            }
            catch (InvalidCastException exception)
            {
                throw new ArgumentException("The destination array type is not compatible with the collection element type.", nameof(array), exception);
            }
            catch (ArrayTypeMismatchException exception)
            {
                throw new ArgumentException("The destination array type is not compatible with the collection element type.", nameof(array), exception);
            }
        }

        /// <summary>Adding is not supported.</summary>
        /// <param name="value">Value that would be added.</param>
        /// <returns>This method does not return because the collection is read-only.</returns>
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
        /// <param name="value">Value to search for.</param>
        /// <returns><c>true</c> if the value exists in the collection.</returns>
        public bool Contains(object value)
        {
            if (value is not T t)
            {
                return false;
            }

            return this.array.Contains(t);
        }

        /// <summary>Gets the index of the provided value or -1 if the value is not present.</summary>
        /// <param name="value">Value to search for.</param>
        /// <returns>The zero-based index of the value, or -1 when not found.</returns>
        public int IndexOf(object value)
        {
            if (value is not T t)
            {
                return -1;
            }

            return this.array.IndexOf(t);
        }

        /// <summary>Insertion is not supported.</summary>
        /// <param name="index">Index at which the insertion would occur.</param>
        /// <param name="value">Value that would be inserted.</param>
        public void Insert(int index, object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Removal is not supported.</summary>
        /// <param name="value">Value that would be removed.</param>
        public void Remove(object value)
        {
            throw new InvalidOperationException("Write not support");
        }

        /// <summary>Removal is not supported.</summary>
        /// <param name="index">Index that would be removed.</param>
        public void RemoveAt(int index)
        {
            throw new InvalidOperationException("Write not support");
        }
    }
}
