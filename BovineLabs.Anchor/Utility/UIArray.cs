namespace BovineLabs.Anchor
{
    using System;
    using System.Collections;
    using Unity.Collections;

    /// <summary>
    /// Read-only IList view of native memory; mutation methods are unsupported.
    /// </summary>
    public class UIArray<T> : IList
        where T : unmanaged, IEquatable<T>
    {
        private NativeArray<T>.ReadOnly array;

        public UIArray(MultiContainer<T> array)
        {
            this.array = array.AsArray();
        }

        public int Count => this.array.IsCreated ? this.array.Length : 0;

        public bool IsSynchronized => true;

        public object SyncRoot { get; } = new();

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public object this[int index]
        {
            get => this.array[index];
            set => throw new InvalidOperationException("Write not supported");
        }

        public static implicit operator UIArray<T>(MultiContainer<T> list)
        {
            return new UIArray<T>(list);
        }

        public IEnumerator GetEnumerator()
        {
            return this.array.GetEnumerator();
        }

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
