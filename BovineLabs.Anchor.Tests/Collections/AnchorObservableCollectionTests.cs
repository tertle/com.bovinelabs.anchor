// <copyright file="AnchorObservableCollectionTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Collections
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using BovineLabs.Anchor.Collections;
    using NUnit.Framework;

    public class AnchorObservableCollectionTests
    {
        [Test]
        public void Replace_WithNull_IsNoOp()
        {
            var collection = new AnchorObservableCollection<int> { 1, 2 };
            var resetCount = CountResetEvents(collection);

            collection.Replace(null);

            Assert.AreEqual(0, resetCount.Count);
            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void Replace_WithChanges_RaisesSingleReset()
        {
            var collection = new AnchorObservableCollection<int> { 1, 2 };
            var resetCount = CountResetEvents(collection);

            collection.Replace(new[] { 5, 6, 7 });

            Assert.AreEqual(1, resetCount.Count);
            Assert.AreEqual(3, collection.Count);
            CollectionAssert.AreEqual(new[] { 5, 6, 7 }, collection);
        }

        [Test]
        public void AddRange_WithNull_IsNoOp()
        {
            var collection = new AnchorObservableCollection<int>();
            var resetCount = CountResetEvents(collection);

            collection.AddRange(null);

            Assert.AreEqual(0, resetCount.Count);
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void AddRange_RaisesSingleReset_OnlyWhenItemsAdded()
        {
            var collection = new AnchorObservableCollection<int>();
            var resetCount = CountResetEvents(collection);

            collection.AddRange(new List<int>());
            Assert.AreEqual(0, resetCount.Count);

            collection.AddRange(new[] { 9, 10 });
            Assert.AreEqual(1, resetCount.Count);
            CollectionAssert.AreEqual(new[] { 9, 10 }, collection);
        }

        private static ResetCounter CountResetEvents(AnchorObservableCollection<int> collection)
        {
            var counter = new ResetCounter();
            collection.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    counter.Count++;
                }
            };

            return counter;
        }

        private sealed class ResetCounter
        {
            public int Count;
        }
    }
}
