// <copyright file="IBindingObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using AOT;
    using BovineLabs.Core.Internal;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;
    using UnityEngine.Assertions;
    using UnityEngine.UIElements;

    internal unsafe delegate void SetValueDelegate(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length);

    internal unsafe delegate void SetListValueDelegate(
        IntPtr target, in FixedString64Bytes property, UntypedUnsafeList* field, void* newValues, int length, int elementSize);

    internal delegate void NotifyDelegate(IntPtr target, in FixedString64Bytes property);

    public interface IBindingObjectNotify : IBindingObject, INotifyBindablePropertyChanged
    {
        void OnPropertyChanging(in FixedString64Bytes property);

        void OnPropertyChanged(in FixedString64Bytes property);

        internal static unsafe class Active
        {
            public static readonly Dictionary<IntPtr, IBindingObjectNotify> Changed = new();

            static Active()
            {
                BurstObjectNotify.SetValue.Data =
                    new FunctionPointer<SetValueDelegate>(Marshal.GetFunctionPointerForDelegate<SetValueDelegate>(SetValueForwarding));

                BurstObjectNotify.SetListValue.Data =
                    new FunctionPointer<SetListValueDelegate>(Marshal.GetFunctionPointerForDelegate<SetListValueDelegate>(SetValueForwarding));

                BurstObjectNotify.Notify.Data = new FunctionPointer<NotifyDelegate>(Marshal.GetFunctionPointerForDelegate<NotifyDelegate>(NotifyForwarding));
            }

            [MonoPInvokeCallback(typeof(SetValueDelegate))]
            private static void SetValueForwarding(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length)
            {
                if (Changed.TryGetValue(target, out var notify))
                {
                    notify.OnPropertyChanging(property);
                    WriteValue();
                    notify.OnPropertyChanged(property);
                }
                else
                {
                    UnsafeUtility.MemCpy(field, newValue, length);
                }

                return;

                void WriteValue()
                {
                    Assert.IsTrue(field != newValue, "Trying to write self to destination");
                    UnsafeUtility.MemCpy(field, newValue, length);
                }
            }

            [MonoPInvokeCallback(typeof(SetListValueDelegate))]
            private static void SetValueForwarding(
                IntPtr target, in FixedString64Bytes property, UntypedUnsafeList* field, void* newValues, int length, int elementSize)
            {
                if (Changed.TryGetValue(target, out var notify))
                {
                    notify.OnPropertyChanging(property);
                    WriteValue();
                    notify.OnPropertyChanged(property);
                }
                else
                {
                    WriteValue();
                }

                return;

                void WriteValue()
                {
                    Assert.IsTrue(field != newValues, "Trying to write self to destination");

                    field->Resize(length, elementSize);
                    UnsafeUtility.MemCpy(field->Ptr, newValues, length * elementSize);
                }
            }

            [MonoPInvokeCallback(typeof(NotifyDelegate))]
            private static void NotifyForwarding(IntPtr target, in FixedString64Bytes property)
            {
                if (Changed.TryGetValue(target, out var notify))
                {
                    notify.OnPropertyChanged(property);
                }
            }
        }
    }

    public interface IBindingObjectNotify<T> : IBindingObject<T>, IBindingObjectNotify
        where T : unmanaged
    {
        private static unsafe void Load(IBindingObjectNotify<T> bindingObjectNotify)
        {
            Active.Changed[(IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value)] = bindingObjectNotify;
        }

        private static unsafe void Unload(IBindingObjectNotify<T> bindingObjectNotify)
        {
            Active.Changed.Remove((IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value));
        }

        /// <inheritdoc />
        void IBindingObject.Load()
        {
            Load(this);
        }

        /// <inheritdoc />
        void IBindingObject.Unload()
        {
            Unload(this);
        }
    }

    internal static class BurstObjectNotify
    {
        public static readonly SharedStatic<FunctionPointer<SetValueDelegate>> SetValue =
            SharedStatic<FunctionPointer<SetValueDelegate>>.GetOrCreate<FunctionPointer<SetValueDelegate>>();

        public static readonly SharedStatic<FunctionPointer<SetListValueDelegate>> SetListValue =
            SharedStatic<FunctionPointer<SetListValueDelegate>>.GetOrCreate<FunctionPointer<SetListValueDelegate>>();

        public static readonly SharedStatic<FunctionPointer<NotifyDelegate>> Notify =
            SharedStatic<FunctionPointer<NotifyDelegate>>.GetOrCreate<FunctionPointer<NotifyDelegate>>();
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct UntypedUnsafeList
    {
        // <WARNING>
        // 'Header' of this struct must binary match `UntypedUnsafeList`, `UnsafeList`.
        [NativeDisableUnsafePtrRestriction]
        internal void* Ptr;
        internal int Length;
        internal int Capacity;
        internal AllocatorManager.AllocatorHandle Allocator;
        internal int Padding;

        internal void Resize(int length, int elementSize)
        {
            if (length > this.Capacity)
            {
                this.SetCapacity(length, elementSize);
            }

            this.Length = length;
        }

        internal void SetCapacity(int capacity, int elementSize)
        {
            var newCapacity = math.max(capacity, CollectionHelper.CacheLineSize / elementSize);
            newCapacity = math.ceilpow2(newCapacity);

            if (newCapacity == this.Capacity)
            {
                return;
            }

            newCapacity = math.max(0, newCapacity);

            void* newPointer = null;

            var alignOf = UnsafeUtility.AlignOf<byte>();

            if (newCapacity > 0)
            {
                newPointer = this.Allocator.Allocate(elementSize, alignOf, newCapacity);

                if (this.Ptr != null && this.Capacity > 0)
                {
                    var itemsToCopy = math.min(newCapacity, this.Capacity);
                    var bytesToCopy = itemsToCopy * elementSize;
                    UnsafeUtility.MemCpy(newPointer, this.Ptr, bytesToCopy);
                }
            }

            AllocatorManager.Free(this.Allocator, this.Ptr, elementSize, alignOf, this.Capacity);

            this.Ptr = newPointer;
            this.Capacity = newCapacity;
        }
    }
}
#endif
