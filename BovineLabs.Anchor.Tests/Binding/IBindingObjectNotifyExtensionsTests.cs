// <copyright file="IBindingObjectNotifyExtensionsTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System;
    using BovineLabs.Anchor.Binding;
    using NUnit.Framework;
    using BovineLabs.Anchor.MVVM;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe class IBindingObjectNotifyExtensionsTests
    {
        [SetUp]
        public void SetUp()
        {
            BurstUIInterop.Changed.Clear();
        }

        [Test]
        public void Load_RegistersObjectInBurstChangedMap()
        {
            var notify = new TestBindingNotify();
            var key = (IntPtr)UnsafeUtility.AddressOf(ref notify.Value);

            notify.Load();

            Assert.IsTrue(BurstUIInterop.Changed.TryGetValue(key, out var resolved));
            Assert.AreSame(notify, resolved);
        }

        [Test]
        public void Unload_RemovesObjectFromBurstChangedMap()
        {
            var notify = new TestBindingNotify();
            var key = (IntPtr)UnsafeUtility.AddressOf(ref notify.Value);

            notify.Load();
            notify.Unload();

            Assert.IsFalse(BurstUIInterop.Changed.ContainsKey(key));
        }

        private sealed class TestBindingNotify : ObservableObject, IBindingObjectNotify<TestData>
        {
            private TestData data;

            public ref TestData Value => ref this.data;

            public void OnPropertyChanging(in FixedString64Bytes property)
            {
            }

            public void OnPropertyChanged(in FixedString64Bytes property)
            {
            }
        }

        private struct TestData
        {
            public int Value;
        }
    }
}

