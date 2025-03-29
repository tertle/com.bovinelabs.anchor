// <copyright file="MultiContainer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Diagnostics;
    using Unity.Assertions;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    // [StructLayout(LayoutKind.Explicit)] // this broke the assembly
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

        public bool IsCreated => this.type switch
        {
            ContainerType.Array => this.array.IsCreated,
            ContainerType.HashSet => this.hashSet.IsCreated,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public int Length
        {
            get
            {
                Assert.IsTrue(this.type == ContainerType.Array);
                return this.array.Length;
            }
        }

        public T this[int index]
        {
            get
            {
                Assert.IsTrue(this.type == ContainerType.Array);
                return this.array[index];
            }
        }

        public static implicit operator MultiContainer<T>(NativeArray<T> array)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = array.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeArray<T>.ReadOnly array)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = array,
            };
        }

        public static implicit operator MultiContainer<T>(NativeList<T> list)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = list.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(DynamicBuffer<T> list)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.Array,
                array = list.AsNativeArray().AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeHashSet<T> hashSet)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.HashSet,
                hashSet = hashSet.AsReadOnly(),
            };
        }

        public static implicit operator MultiContainer<T>(NativeHashSet<T>.ReadOnly hashSet)
        {
            return new MultiContainer<T>
            {
                type = ContainerType.HashSet,
                hashSet = hashSet,
            };
        }

        public NativeArray<T>.ReadOnly AsArray()
        {
            Assert.IsTrue(this.type == ContainerType.Array);
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
