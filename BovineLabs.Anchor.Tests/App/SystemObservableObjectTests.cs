// <copyright file="SystemObservableObjectTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Unity.Collections;

    public class SystemObservableObjectTests
    {
        [Test]
        public void FixedStringNotifications_RaisePropertyEvents()
        {
            var observable = new TestSystemObservableObject();
            var changing = new List<string>();
            var changed = new List<string>();

            observable.PropertyChanging += (_, args) => changing.Add(args.PropertyName);
            observable.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            observable.OnPropertyChanging(new FixedString64Bytes("Count"));
            observable.OnPropertyChanged(new FixedString64Bytes("Count"));

            CollectionAssert.AreEqual(new[] { "Count" }, changing);
            CollectionAssert.AreEqual(new[] { "Count" }, changed);
        }

        [Test]
        public void SetProperty_UnchangedList_ReturnsFalse()
        {
            var observable = new TestSystemObservableObject();
            var changing = 0;
            var changed = 0;

            observable.PropertyChanging += (_, _) => changing++;
            observable.PropertyChanged += (_, _) => changed++;

            var list = new NativeList<int>(Allocator.Temp);
            list.Add(1);
            ChangedList<int> oldValue = list;

            var result = observable.InvokeSetProperty(oldValue, new[] { 1 }, "Items");

            Assert.IsFalse(result);
            Assert.AreEqual(0, changing);
            Assert.AreEqual(0, changed);
            Assert.AreEqual(1, list.Length);
            Assert.AreEqual(1, list[0]);
        }

        [Test]
        public void SetProperty_ChangedList_ReturnsTrueAndMutates()
        {
            var observable = new TestSystemObservableObject();
            var changing = 0;
            var changed = 0;

            observable.PropertyChanging += (_, _) => changing++;
            observable.PropertyChanged += (_, _) => changed++;

            var list = new NativeList<int>(Allocator.Temp);
            list.Add(1);
            ChangedList<int> oldValue = list;

            var result = observable.InvokeSetProperty(oldValue, new[] { 1, 2 }, "Items");

            Assert.IsTrue(result);
            Assert.AreEqual(1, changing);
            Assert.AreEqual(1, changed);
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
        }

        private sealed class TestSystemObservableObject : SystemObservableObject<TestData>
        {
            public bool InvokeSetProperty(ChangedList<int> oldValue, IEnumerable<int> newValue, string propertyName)
            {
                return this.SetProperty(oldValue, newValue, propertyName);
            }
        }

        private struct TestData
        {
            public int Value;
        }
    }
}
