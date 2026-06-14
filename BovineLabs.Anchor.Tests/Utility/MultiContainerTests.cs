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
