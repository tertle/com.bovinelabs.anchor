// <copyright file="AnchorObservableCollectionTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using BovineLabs.Anchor.Collections;
    using NUnit.Framework;

    public class AnchorObservableCollectionTests
    {
        [Test]
        public void Replace_WithChanges_RaisesSingleReset()
        {
            var collection = new AnchorObservableCollection<int> { 1, 2 };
            var events = ObserveEvents(collection);

            collection.Replace(new[] { 5, 6, 7 });

            CollectionAssert.AreEqual(new[] { "Count", "Item[]", "Reset" }, events);
            Assert.AreEqual(3, collection.Count);
            CollectionAssert.AreEqual(new[] { 5, 6, 7 }, collection);
        }

        [Test]
        public void AddRange_RaisesSingleReset_OnlyWhenItemsAdded()
        {
            var collection = new AnchorObservableCollection<int>();
            var events = ObserveEvents(collection);

            collection.AddRange(new List<int>());
            Assert.AreEqual(0, events.Count);

            collection.AddRange(new[] { 9, 10 });
            CollectionAssert.AreEqual(new[] { "Count", "Item[]", "Reset" }, events);
            CollectionAssert.AreEqual(new[] { 9, 10 }, collection);
        }

        [Test]
        public void AddRange_WithSelf_SnapshotsOriginalItems()
        {
            var collection = new AnchorObservableCollection<int> { 1, 2 };
            var events = ObserveEvents(collection);

            collection.AddRange(collection);

            CollectionAssert.AreEqual(new[] { 1, 2, 1, 2 }, collection);
            CollectionAssert.AreEqual(new[] { "Count", "Item[]", "Reset" }, events);
        }

        [Test]
        public void Replace_WithSelf_SnapshotsOriginalItems()
        {
            var collection = new AnchorObservableCollection<int> { 1, 2 };
            var events = ObserveEvents(collection);

            collection.Replace(collection);

            CollectionAssert.AreEqual(new[] { 1, 2 }, collection);
            CollectionAssert.AreEqual(new[] { "Count", "Item[]", "Reset" }, events);
        }

        private static List<string> ObserveEvents(AnchorObservableCollection<int> collection)
        {
            var events = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (_, args) => events.Add(args.PropertyName);
            collection.CollectionChanged += (_, args) => events.Add(args.Action.ToString());

            return events;
        }
    }
}
