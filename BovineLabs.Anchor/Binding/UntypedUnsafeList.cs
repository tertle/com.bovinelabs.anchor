// <copyright file="UntypedUnsafeList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct UntypedUnsafeList
    {
        // <WARNING>
        // 'Header' of this struct must binary match `UntypedUnsafeList`, `UnsafeList`.
        [NativeDisableUnsafePtrRestriction]
        internal void* Ptr;
        internal int Length;
        internal int Capacity;
        internal AllocatorManager.AllocatorHandle Allocator;
        internal int Padding;

        internal void Resize(int length, int elementSize)
        {
            if (length > this.Capacity)
            {
                this.SetCapacity(length, elementSize);
            }

            this.Length = length;
        }

        internal void SetCapacity(int capacity, int elementSize)
        {
            var newCapacity = math.max(capacity, CollectionHelper.CacheLineSize / elementSize);
            newCapacity = math.ceilpow2(newCapacity);

            if (newCapacity == this.Capacity)
            {
                return;
            }

            newCapacity = math.max(0, newCapacity);

            void* newPointer = null;

            var alignOf = UnsafeUtility.AlignOf<byte>();

            if (newCapacity > 0)
            {
                newPointer = this.Allocator.Allocate(elementSize, alignOf, newCapacity);

                if (this.Ptr != null && this.Capacity > 0)
                {
                    var itemsToCopy = math.min(newCapacity, this.Capacity);
                    var bytesToCopy = itemsToCopy * elementSize;
                    UnsafeUtility.MemCpy(newPointer, this.Ptr, bytesToCopy);
                }
            }

            AllocatorManager.Free(this.Allocator, this.Ptr, elementSize, alignOf, this.Capacity);

            this.Ptr = newPointer;
            this.Capacity = newCapacity;
        }
    }
}