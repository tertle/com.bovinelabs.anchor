// <copyright file="UIArrayTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using System;
    using NUnit.Framework;
    using Unity.Collections;

    public class UIArrayTests
    {
        [Test]
        public void WriteOperations_ThrowInvalidOperationException()
        {
            var nativeArray = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;

            Assert.Throws<InvalidOperationException>(() => uiArray.Add(4));
            Assert.Throws<InvalidOperationException>(() => uiArray.Clear());
            Assert.Throws<InvalidOperationException>(() => uiArray.Insert(0, 10));
            Assert.Throws<InvalidOperationException>(() => uiArray.Remove(1));
            Assert.Throws<InvalidOperationException>(() => uiArray.RemoveAt(0));
        }

        [Test]
        public void ContainsAndIndexOf_ReturnExpectedValues_AndHandleTypeMismatch()
        {
            var nativeArray = new NativeArray<int>(new[] { 4, 5, 6 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;

            Assert.IsTrue(uiArray.Contains(5));
            Assert.IsFalse(uiArray.Contains(99));
            Assert.IsFalse(uiArray.Contains("5"));

            Assert.AreEqual(2, uiArray.IndexOf(6));
            Assert.AreEqual(-1, uiArray.IndexOf(100));
            Assert.AreEqual(-1, uiArray.IndexOf("6"));
        }

        [Test]
        public void CopyTo_WithOffset_CopiesEveryElement()
        {
            var nativeArray = new NativeArray<int>(new[] { 4, 5, 6 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;
            var destination = new[] { 1, 2, 3, 3, 3 };

            uiArray.CopyTo(destination, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 4, 5, 6 }, destination);
        }

        [Test]
        public void CopyTo_ObjectArray_BoxesEveryElement()
        {
            var nativeArray = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;
            var destination = new object[2];

            uiArray.CopyTo(destination, 0);

            CollectionAssert.AreEqual(new object[] { 4, 5 }, destination);
        }

        [Test]
        public void CopyTo_InvalidDestination_ThrowsExpectedArgumentException()
        {
            var nativeArray = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;

            Assert.Throws<ArgumentNullException>(() => uiArray.CopyTo(null, 0));
            Assert.Throws<ArgumentException>(() => uiArray.CopyTo(new int[1, 2], 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => uiArray.CopyTo(new int[2], -1));
            Assert.Throws<ArgumentException>(() => uiArray.CopyTo(new int[2], 1));
            Assert.Throws<ArgumentException>(() => uiArray.CopyTo(new string[2], 0));
        }
    }
}
