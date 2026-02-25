// <copyright file="BurstUIInteropTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Binding;
    using NUnit.Framework;
    using BovineLabs.Anchor.MVVM;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe class BurstUIInteropTests
    {
        [SetUp]
        public void SetUp()
        {
            BurstUIInterop.Changed.Clear();
        }

        [Test]
        public void StaticInitialization_InitializesForwardingPointers()
        {
            _ = BurstUIInterop.Changed.Count;

            Assert.IsTrue(BurstObjectNotify.SetValue.Data.IsCreated);
            Assert.IsTrue(BurstObjectNotify.SetListValue.Data.IsCreated);
            Assert.IsTrue(BurstObjectNotify.Notify.Data.IsCreated);
        }

        [Test]
        public void NotifyForwarding_UnknownTarget_IsNoOp()
        {
            Assert.DoesNotThrow(() =>
            {
                BurstObjectNotify.Notify.Data.Invoke(new IntPtr(987), new FixedString64Bytes("Value"));
            });
        }

        [Test]
        public void SetValueForwarding_UnregisteredTarget_WritesMemory()
        {
            var target = new IntPtr(1234);
            var property = new FixedString64Bytes("Value");
            var field = 10;
            var newValue = 42;

            BurstObjectNotify.SetValue.Data.Invoke(
                target,
                property,
                UnsafeUtility.AddressOf(ref field),
                UnsafeUtility.AddressOf(ref newValue),
                sizeof(int));

            Assert.AreEqual(42, field);
        }

        [Test]
        public void SetValueForwarding_RegisteredTarget_NotifiesAroundWrite()
        {
            var target = new IntPtr(4567);
            var receiver = new TestBindingNotifyReceiver();
            BurstUIInterop.Changed[target] = receiver;

            var property = new FixedString64Bytes("Field");
            var field = 1;
            var newValue = 7;

            BurstObjectNotify.SetValue.Data.Invoke(
                target,
                property,
                UnsafeUtility.AddressOf(ref field),
                UnsafeUtility.AddressOf(ref newValue),
                sizeof(int));

            Assert.AreEqual(7, field);
            CollectionAssert.AreEqual(
                new[] { "changing:Field", "changed:Field" },
                receiver.Calls);
        }

        private sealed class TestBindingNotifyReceiver : ObservableObject, IBindingObjectNotify
        {
            public List<string> Calls { get; } = new();

            public void OnPropertyChanging(in FixedString64Bytes property)
            {
                this.Calls.Add($"changing:{property}");
            }

            public void OnPropertyChanged(in FixedString64Bytes property)
            {
                this.Calls.Add($"changed:{property}");
            }
        }
    }
}

