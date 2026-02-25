// <copyright file="ToolbarViewModelTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_APPUI
namespace BovineLabs.Anchor.Tests.Toolbar
{
    using System.Linq;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class ToolbarViewModelTests
    {
        private const string SelectionKey = "bl.toolbarmanager.filter.selections";

        [Test]
        public void Constructor_LoadsPersistedHiddenSelections()
        {
            var storage = new TestLocalStorageService();
            storage.SetValue(SelectionKey, "one,two");

            var vm = new ToolbarViewModel(storage);

            CollectionAssert.AreEquivalent(new[] { "one", "two" }, vm.SelectionsHidden);
        }

        [Test]
        public void AddSelection_AddsOnce_AndSortsFilterItems()
        {
            var vm = new ToolbarViewModel(new TestLocalStorageService());

            vm.AddSelection("Zulu");
            vm.AddSelection("Alpha");

            CollectionAssert.AreEqual(new[] { "Alpha", "Zulu" }, vm.FilterItems);
            CollectionAssert.AreEqual(new[] { 0, 1 }, vm.FilterValues.ToArray());
        }

        [Test]
        public void AddSelection_Repeated_DoesNotDuplicateItems()
        {
            var vm = new ToolbarViewModel(new TestLocalStorageService());

            vm.AddSelection("Shared");
            vm.AddSelection("Shared");

            Assert.AreEqual(1, vm.FilterItems.Count);
            Assert.AreEqual("Shared", vm.FilterItems[0]);
        }

        [Test]
        public void RemoveSelection_DecrementsAndRemovesWhenRefCountHitsZero()
        {
            var vm = new ToolbarViewModel(new TestLocalStorageService());
            vm.AddSelection("Shared");
            vm.AddSelection("Shared");

            vm.RemoveSelection("Shared");
            Assert.AreEqual(1, vm.FilterItems.Count);

            vm.RemoveSelection("Shared");
            Assert.AreEqual(0, vm.FilterItems.Count);
        }

        [Test]
        public void FilterValues_UpdatesHiddenSet_AndPersists()
        {
            var storage = new TestLocalStorageService();
            var vm = new ToolbarViewModel(storage);
            vm.AddSelection("A");
            vm.AddSelection("B");
            vm.AddSelection("C");

            vm.FilterValues = new[] { 0, 2 };

            Assert.IsFalse(vm.SelectionsHidden.Contains("A"));
            Assert.IsTrue(vm.SelectionsHidden.Contains("B"));
            Assert.IsFalse(vm.SelectionsHidden.Contains("C"));
            Assert.AreEqual(SelectionKey, storage.LastSetStringKey);
            Assert.AreEqual("B", storage.LastSetStringValue);
        }

        [Test]
        public void FilterValues_EqualSequence_IsNoOp()
        {
            var storage = new TestLocalStorageService();
            var vm = new ToolbarViewModel(storage);
            vm.AddSelection("A");
            vm.FilterValues = new[] { 0 };

            var before = storage.SetStringValueCount;
            vm.FilterValues = new[] { 0 };

            Assert.AreEqual(before, storage.SetStringValueCount);
        }

        [Test]
        public void RefreshItems_FromAddRemove_KeepsVisibleFiltersActive()
        {
            var storage = new TestLocalStorageService();
            storage.SetValue(SelectionKey, "B");

            var vm = new ToolbarViewModel(storage);
            vm.AddSelection("A");
            vm.AddSelection("B");
            vm.AddSelection("C");

            CollectionAssert.AreEqual(new[] { 0, 2 }, vm.FilterValues.ToArray());

            vm.RemoveSelection("A");

            CollectionAssert.AreEqual(new[] { "B", "C" }, vm.FilterItems);
            CollectionAssert.AreEqual(new[] { 1 }, vm.FilterValues.ToArray());
            Assert.IsTrue(vm.SelectionsHidden.Contains("B"));
            Assert.IsFalse(vm.SelectionsHidden.Contains("C"));
        }
    }
}
#endif
