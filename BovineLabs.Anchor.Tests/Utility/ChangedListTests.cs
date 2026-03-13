// <copyright file="ChangedListTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using NUnit.Framework;
    using Unity.Collections;

    public class ChangedListTests
    {
        [Test]
        public void AddAndSetValue_MarkAsChanged()
        {
            var nativeList = new NativeList<int>(Allocator.Temp);
            ChangedList<int> changedList = nativeList;

            Assert.IsTrue(changedList.GetIfChanged(out _));
            Assert.IsFalse(changedList.GetIfChanged(out _));

            changedList.Add(3);
            Assert.IsTrue(changedList.GetIfChanged(out var afterAdd));
            Assert.AreEqual(1, afterAdd.Length);
            Assert.AreEqual(3, afterAdd[0]);
            Assert.IsFalse(changedList.GetIfChanged(out _));

            changedList.SetValue(new[] { 8, 9 });
            Assert.IsTrue(changedList.GetIfChanged(out var afterSet));
            Assert.AreEqual(2, afterSet.Length);
            Assert.AreEqual(8, afterSet[0]);
            Assert.AreEqual(9, afterSet[1]);
        }

        [Test]
        public void GetIfChanged_ReturnsTrueOncePerMutation()
        {
            var nativeList = new NativeList<int>(Allocator.Temp);
            ChangedList<int> changedList = nativeList;

            Assert.IsTrue(changedList.GetIfChanged(out _));
            Assert.IsFalse(changedList.GetIfChanged(out _));

            changedList.Add(42);

            Assert.IsTrue(changedList.GetIfChanged(out var value));
            Assert.AreEqual(1, value.Length);
            Assert.AreEqual(42, value[0]);
            Assert.IsFalse(changedList.GetIfChanged(out _));
        }
    }
}
