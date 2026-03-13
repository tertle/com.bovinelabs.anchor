// <copyright file="MultiContainerTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using NUnit.Framework;
    using Unity.Collections;

    public class MultiContainerTests
    {
        [Test]
        public void ImplicitConversions_PreserveData()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var list = new NativeList<int>(Allocator.Temp);
            var hashSet = new NativeHashSet<int>(3, Allocator.Temp);
            var hashSetMatch = new NativeArray<int>(new[] { 7, 6 }, Allocator.Temp);
            var hashSetMismatch = new NativeArray<int>(new[] { 7, 8 }, Allocator.Temp);

            list.Add(4);
            list.Add(5);
            hashSet.Add(6);
            hashSet.Add(7);

            MultiContainer<int> fromArray = array;
            MultiContainer<int> fromList = list;
            MultiContainer<int> fromHashSet = hashSet;

            Assert.IsTrue(fromArray.IsCreated);
            Assert.AreEqual(3, fromArray.Length);
            Assert.AreEqual(2, fromArray[1]);

            Assert.IsTrue(fromList.IsCreated);
            Assert.AreEqual(2, fromList.Length);
            Assert.AreEqual(4, fromList[0]);
            Assert.AreEqual(5, fromList[1]);

            Assert.IsTrue(fromHashSet.ArraysEqual(hashSetMatch));
            Assert.IsFalse(fromHashSet.ArraysEqual(hashSetMismatch));
        }

        [Test]
        public void ArraysEqual_WorksForArrayBackedAndHashSetBackedContainers()
        {
            var array = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
            var sameArray = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
            var differentArray = new NativeArray<int>(new[] { 10, 20, 31 }, Allocator.Temp);
            var hashSet = new NativeHashSet<int>(3, Allocator.Temp);
            var sameHashSetValues = new NativeArray<int>(new[] { 2, 1, 3 }, Allocator.Temp);
            var differentHashSetValues = new NativeArray<int>(new[] { 2, 1 }, Allocator.Temp);
            MultiContainer<int> arrayContainer = array;
            MultiContainer<int> hashSetContainer = hashSet;

            hashSet.Add(1);
            hashSet.Add(2);
            hashSet.Add(3);

            Assert.IsTrue(arrayContainer.ArraysEqual(sameArray));
            Assert.IsFalse(arrayContainer.ArraysEqual(differentArray));

            Assert.IsTrue(hashSetContainer.ArraysEqual(sameHashSetValues));
            Assert.IsFalse(hashSetContainer.ArraysEqual(differentHashSetValues));
        }
    }
}
