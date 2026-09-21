namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Debug = UnityEngine.Debug;
    using Unity.Entities;

    // [StructLayout(LayoutKind.Explicit)] // this broke the assembly
    public struct MultiContainer<T>
        where T : unmanaged, IEquatable<T>
    {
        private ContainerType _type;
        private NativeArray<T>.ReadOnly _array;
        private NativeHashSet<T>.ReadOnly _hashSet;

        private enum ContainerType
        {
            // No 0 to ensure initialization
            Array = 1,
            HashSet = 2,
        }

        public bool IsCreated => _type switch
        {
            ContainerType.Array => _array.IsCreated,
            ContainerType.HashSet => _hashSet.IsCreated,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public int Length
        {
            get
            {
                Debug.Assert(_type == ContainerType.Array, "Length used on non array");
                return _array.Length;
            }
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(_type == ContainerType.Array, "Indexer used on non array");
                return _array[index];
            }
        }

        public static implicit operator MultiContainer<T>(NativeArray<T> array)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.Array,
                _array = array.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeArray<T>.ReadOnly array)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.Array,
                _array = array,
            };
        }

        public static implicit operator MultiContainer<T>(NativeList<T> list)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.Array,
                _array = list.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(DynamicBuffer<T> list)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.Array,
                _array = list.AsNativeArray().AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeHashSet<T> hashSet)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.HashSet,
                _hashSet = hashSet.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeHashSet<T>.ReadOnly hashSet)
        {
            return new MultiContainer<T>
            {
                _type = ContainerType.HashSet,
                _hashSet = hashSet,
            };
        }

        public NativeArray<T>.ReadOnly AsArray()
        {
            Debug.Assert(_type == ContainerType.Array, "AsArray used on non array");
            return _array;
        }

        internal bool ArraysEqual(NativeArray<T> other)
        {
            switch (_type)
            {
                case ContainerType.Array:
                    if (_array.Length != other.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i != _array.Length; i++)
                    {
                        if (!_array[i].Equals(other[i]))
                        {
                            return false;
                        }
                    }

                    return true;

                case ContainerType.HashSet:

                    if (other.Length != _hashSet.Count)
                    {
                        return false;
                    }

                    foreach (var l in other)
                    {
                        if (!_hashSet.Contains(l))
                        {
                            return false;
                        }
                    }

                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal unsafe (IntPtr Ptr, int Length) GetAsTempArray()
        {
            return _type switch
            {
                ContainerType.Array => ((IntPtr)_array.GetUnsafeReadOnlyPtr(), _array.Length),
                ContainerType.HashSet => ((IntPtr)_hashSet.ToNativeArray(Allocator.Temp).GetUnsafeReadOnlyPtr(), _hashSet.Count),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal unsafe void ThrowContainersMatch(NativeList<T> list)
        {
            if (_type == ContainerType.Array)
            {
                if (_array.GetUnsafeReadOnlyPtr() == list.GetUnsafeReadOnlyPtr())
                {
                    throw new InvalidOperationException("Containers match");
                }
            }
        }
    }
}
