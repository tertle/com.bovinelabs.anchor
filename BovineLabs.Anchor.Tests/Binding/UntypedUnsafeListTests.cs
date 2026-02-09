// <copyright file="UntypedUnsafeListTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System;
    using BovineLabs.Anchor.Binding;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe class UntypedUnsafeListTests
    {
        [Test]
        public void ResizeAndSetCapacity_GrowAndShrink_PreservesLeadingData()
        {
            var list = new UntypedUnsafeList
            {
                Allocator = Allocator.Persistent,
            };

            const int elementSize = sizeof(int);

            try
            {
                list.Resize(3, elementSize);
                Assert.GreaterOrEqual(list.Capacity, 3);

                var data = (int*)list.Ptr;
                data[0] = 10;
                data[1] = 20;
                data[2] = 30;

                var grownRequest = list.Capacity * 2;
                list.SetCapacity(grownRequest, elementSize);
                Assert.GreaterOrEqual(list.Capacity, grownRequest);

                data = (int*)list.Ptr;
                Assert.AreEqual(10, data[0]);
                Assert.AreEqual(20, data[1]);
                Assert.AreEqual(30, data[2]);

                var previousCapacity = list.Capacity;
                list.SetCapacity(1, elementSize);
                Assert.LessOrEqual(list.Capacity, previousCapacity);
                Assert.GreaterOrEqual(list.Capacity, 1);

                data = (int*)list.Ptr;
                Assert.AreEqual(10, data[0]);
                Assert.AreEqual(20, data[1]);
                Assert.AreEqual(30, data[2]);
            }
            finally
            {
                Free(ref list, elementSize);
            }
        }

        [Test]
        public void SetCapacity_ZeroPath_ResetsPointerAndCapacity()
        {
            var list = new UntypedUnsafeList
            {
                Allocator = Allocator.Persistent,
            };

            var largeElementSize = CollectionHelper.CacheLineSize * 2;

            try
            {
                list.SetCapacity(1, largeElementSize);

                Assert.AreEqual(1, list.Capacity);
                Assert.AreNotEqual(IntPtr.Zero, (IntPtr)list.Ptr);

                list.SetCapacity(0, largeElementSize);

                Assert.AreEqual(0, list.Capacity);
                Assert.AreEqual(IntPtr.Zero, (IntPtr)list.Ptr);
            }
            finally
            {
                Free(ref list, largeElementSize);
            }
        }

        private static void Free(ref UntypedUnsafeList list, int elementSize)
        {
            if (list.Ptr == null || list.Capacity <= 0)
            {
                return;
            }

            var alignOf = UnsafeUtility.AlignOf<byte>();
            AllocatorManager.Free(list.Allocator, list.Ptr, elementSize, alignOf, list.Capacity);
            list.Ptr = null;
            list.Capacity = 0;
            list.Length = 0;
        }
    }
}
