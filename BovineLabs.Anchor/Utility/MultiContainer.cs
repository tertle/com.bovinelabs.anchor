// <copyright file="MultiContainer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Debug = UnityEngine.Debug;
    using Unity.Entities;

    // [StructLayout(LayoutKind.Explicit)] // this broke the assembly
    /// <summary>
    /// Lightweight wrapper that can represent several native collection types through a single interface.
    /// </summary>
    public struct MultiContainer<T>
        where T : unmanaged, IEquatable<T>
    {
        private ContainerType type;
        private NativeArray<T>.ReadOnly array;
        private NativeHashSet<T>.ReadOnly hashSet;

        private enum ContainerType
        {
            // No 0 to ensure initialization
            Array = 1,
            HashSet = 2,
        }

        /// <summary>Gets a value indicating whether the underlying container has been allocated.</summary>
        public bool IsCreated => this.type switch
        {
            ContainerType.Array => this.array.IsCreated,
            ContainerType.HashSet => this.hashSet.IsCreated,
            _ => throw new ArgumentOutOfRangeException(),
        };

        /// <summary>Gets the length of the underlying array container.</summary>
        public int Length
        {
            get
            {
                Debug.Assert(this.type == ContainerType.Array, "Length used on non array");
                return this.array.Length;
            }
        }

        /// <summary>Provides read-only indexed access when the container wraps an array.</summary>
        public T this[int index]
        {
            get
            {
                Debug.Assert(this.type == ContainerType.Array, "Indexer used on non array");
                return this.array[index];
            }
        }

        /// <summary>Creates a container backed by the provided native array.</summary>
        public static implicit operator MultiContainer<T>(NativeArray<T> array)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = array.AsReadOnly(),
            };
        }

        /// <summary>Creates a container backed by a read-only native array.</summary>
        public static implicit operator MultiContainer<T>(NativeArray<T>.ReadOnly array)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = array,
            };
        }

        /// <summary>Creates a container backed by a native list.</summary>
        public static implicit operator MultiContainer<T>(NativeList<T> list)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = list.AsReadOnly(),
            };
        }

        /// <summary>Creates a container backed by a dynamic buffer.</summary>
        public static implicit operator MultiContainer<T>(DynamicBuffer<T> list)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = list.AsNativeArray().AsReadOnly(),
            };
        }

        /// <summary>Creates a container backed by a native hash set.</summary>
        public static implicit operator MultiContainer<T>(NativeHashSet<T> hashSet)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.HashSet,
                hashSet = hashSet.AsReadOnly(),
            };
        }

        /// <summary>Creates a container backed by a read-only native hash set.</summary>
        public static implicit operator MultiContainer<T>(NativeHashSet<T>.ReadOnly hashSet)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.HashSet,
                hashSet = hashSet,
            };
        }

        /// <summary>Returns the wrapped data as a native array.</summary>
        public NativeArray<T>.ReadOnly AsArray()
        {
            Debug.Assert(this.type == ContainerType.Array, "AsArray used on non array");
            return this.array;
        }

        internal bool ArraysEqual(NativeArray<T> other)
        {
            switch (this.type)
            {
                case ContainerType.Array:
                    if (this.array.Length != other.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i != this.array.Length; i++)
                    {
                        if (!this.array[i].Equals(other[i]))
                        {
                            return false;
                        }
                    }

                    return true;

                case ContainerType.HashSet:

                    if (other.Length != this.hashSet.Count)
                    {
                        return false;
                    }

                    foreach (var l in other)
                    {
                        if (!this.hashSet.Contains(l))
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
            return this.type switch
            {
                ContainerType.Array => ((IntPtr)this.array.GetUnsafeReadOnlyPtr(), this.array.Length),
                ContainerType.HashSet => ((IntPtr)this.hashSet.ToNativeArray(Allocator.Temp).GetUnsafeReadOnlyPtr(), this.hashSet.Count),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal unsafe void ThrowContainersMatch(NativeList<T> list)
        {
            if (this.type == ContainerType.Array)
            {
                if (this.array.GetUnsafeReadOnlyPtr() == list.GetUnsafeReadOnlyPtr())
                {
                    throw new InvalidOperationException("Containers match");
                }
            }
        }
    }
}
