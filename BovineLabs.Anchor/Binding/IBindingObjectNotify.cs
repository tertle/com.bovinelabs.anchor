﻿// <copyright file="IBindingObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using AOT;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.UIElements;

    internal unsafe delegate void SetValueDelegate(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length);

    internal delegate void NotifyDelegate(IntPtr target, in FixedString64Bytes property);

    internal static class BurstObjectNotify
    {
        public static readonly SharedStatic<FunctionPointer<SetValueDelegate>> SetValue =
            SharedStatic<FunctionPointer<SetValueDelegate>>.GetOrCreate<FunctionPointer<SetValueDelegate>>();

        public static readonly SharedStatic<FunctionPointer<NotifyDelegate>> Notify =
            SharedStatic<FunctionPointer<NotifyDelegate>>.GetOrCreate<FunctionPointer<NotifyDelegate>>();
    }

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

                BurstObjectNotify.Notify.Data = new FunctionPointer<NotifyDelegate>(Marshal.GetFunctionPointerForDelegate<NotifyDelegate>(NotifyForwarding));
            }

            [MonoPInvokeCallback(typeof(SetValueDelegate))]
            private static void SetValueForwarding(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length)
            {
                if (Changed.TryGetValue(target, out var notify))
                {
                    notify.OnPropertyChanging(property);
                    UnsafeUtility.MemCpy(field, newValue, length);
                    notify.OnPropertyChanged(property);
                }
                else
                {
                    UnsafeUtility.MemCpy(field, newValue, length);
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
        public static unsafe void Load(IBindingObjectNotify<T> bindingObjectNotify)
        {
            var addr = (IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value);
            Active.Changed[addr] = bindingObjectNotify;
        }

        public static unsafe void Unload(IBindingObjectNotify<T> bindingObjectNotify)
        {
            var addr = (IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value);
            Active.Changed.Remove(addr);
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

    public static class BindingObjectNotifyDataExtensions
    {
        public static bool SetProperty<T, TV>(this ref T binding, ref TV field, TV newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged, IEquatable<TV>
        {
            if (field.Equals(newValue))
            {
                return false;
            }

            SetValue(ref binding, ref field, newValue, propertyName);
            return true;
        }

        // TODO naming confusing
        public static unsafe void SetValue<T, TV>(this ref T binding, ref TV field, TV newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged
        {
            if (BurstObjectNotify.SetValue.Data.IsCreated)
            {
                var target = (IntPtr)UnsafeUtility.AddressOf(ref binding);
                var fieldPtr = UnsafeUtility.AddressOf(ref field);
                var valuePtr = &newValue;

                BurstObjectNotify.SetValue.Data.Invoke(target, propertyName, fieldPtr, valuePtr, sizeof(T));
            }
        }

        public static unsafe void Notify<T>(this ref T binding, FixedString64Bytes propertyName)
            where T : unmanaged
        {
            if (BurstObjectNotify.SetValue.Data.IsCreated)
            {
                var target = (IntPtr)UnsafeUtility.AddressOf(ref binding);
                BurstObjectNotify.Notify.Data.Invoke(target, propertyName);
            }
        }
    }
}
#endif
