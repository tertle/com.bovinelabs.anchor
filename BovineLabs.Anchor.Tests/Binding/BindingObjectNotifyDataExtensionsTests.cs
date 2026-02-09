// <copyright file="BindingObjectNotifyDataExtensionsTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.Collections;

    public class BindingObjectNotifyDataExtensionsTests
    {
        [Test]
        public void SetProperty_SameValue_ReturnsFalseAndDoesNotMutate()
        {
            var binding = new TestBindingObject();
            var field = 11;

            var changed = binding.SetProperty(ref field, 11, "Value");

            Assert.IsFalse(changed);
            Assert.AreEqual(11, field);
        }

        [Test]
        public void SetProperty_ChangedValue_ReturnsTrueAndMutates()
        {
            var binding = new TestBindingObject();
            var field = 11;

            var changed = binding.SetProperty(ref field, 12, "Value");

            Assert.IsTrue(changed);
            Assert.AreEqual(12, field);
        }

        [Test]
        public void SetProperty_ChangedWrapperSameValueAndResetFlag_ResetsHasChanged()
        {
            var binding = new TestBindingObject();
            var field = new Changed<int>(42, true);

            var changed = binding.SetProperty(ref field, new Changed<int>(42, false), "Value");

            Assert.IsFalse(changed);
            Assert.AreEqual(42, field.Value);
            Assert.IsFalse(field.HasChanged);
        }

        [Test]
        public void SetProperty_NativeListEqualContent_ReturnsFalse()
        {
            var binding = new TestBindingObject();
            var list = new NativeList<int>(Allocator.Temp);
            var incoming = new NativeArray<int>(new[] { 1, 2 }, Allocator.Temp);
            MultiContainer<int> incomingContainer = incoming;

            list.Add(1);
            list.Add(2);

            var changed = binding.SetProperty(list, incomingContainer, "Items");

            Assert.IsFalse(changed);
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
        }

        [Test]
        public void SetProperty_NativeListDifferentContent_ReplacesList()
        {
            var binding = new TestBindingObject();
            var list = new NativeList<int>(Allocator.Temp);
            var incoming = new NativeArray<int>(new[] { 7, 8, 9 }, Allocator.Temp);
            MultiContainer<int> incomingContainer = incoming;

            list.Add(1);
            list.Add(2);

            var changed = binding.SetProperty(list, incomingContainer, "Items");

            Assert.IsTrue(changed);
            Assert.AreEqual(3, list.Length);
            Assert.AreEqual(7, list[0]);
            Assert.AreEqual(8, list[1]);
            Assert.AreEqual(9, list[2]);
        }

        [Test]
        public void SetProperty_ChangedListWithSameList_ReturnsTrueAndNotifies()
        {
            var binding = new TestBindingObject();
            var list = new NativeList<int>(Allocator.Temp);
            list.Add(1);

            ChangedList<int> field = list;
            ChangedList<int> incoming = list;

            var changed = binding.SetProperty(ref field, incoming, "Items");

            Assert.IsTrue(changed);
            Assert.IsTrue(field.Value.IsCreated);
            Assert.AreEqual(1, field.Value.Length);
            Assert.AreEqual(1, field.Value[0]);
        }

        [Test]
        public void Notify_WhenTrampolineNotInitialized_IsNoOp()
        {
            var binding = new TestBindingObject();

            Assert.DoesNotThrow(() => binding.Notify("Noop"));
        }
    }
}
