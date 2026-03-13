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
    }
}
