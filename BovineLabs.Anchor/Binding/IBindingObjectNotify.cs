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
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.UIElements;

    internal unsafe delegate void SetValueDelegate(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length);

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

                BurstObjectNotify.Notify.Data = new FunctionPointer<NotifyDelegate>(Marshal.GetFunctionPointerForDelegate<NotifyDelegate>(NotifyForwarding));
            }

            [MonoPInvokeCallback(typeof(SetValueDelegate))]
            private static void SetValueForwarding(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length)
            {
                if (Changed.TryGetValue(target, out var notify))
                {
                    notify.OnPropertyChanging(property);
                    if (field != newValue)
                    {
                        UnsafeUtility.MemCpy(field, newValue, length);
                    }

                    notify.OnPropertyChanged(property);
                }
                else if (field != newValue)
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

        public static readonly SharedStatic<FunctionPointer<NotifyDelegate>> Notify =
            SharedStatic<FunctionPointer<NotifyDelegate>>.GetOrCreate<FunctionPointer<NotifyDelegate>>();
    }
}
#endif
