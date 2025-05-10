// <copyright file="BurstUIInterop.cs" company="BovineLabs">
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
    using UnityEngine.Assertions;

    internal static unsafe class BurstUIInterop
    {
        internal static readonly Dictionary<IntPtr, IBindingObjectNotify> Changed = new();

        static BurstUIInterop()
        {
            BurstObjectNotify.SetValue.Data = new FunctionPointer<SetValueDelegate>(
                Marshal.GetFunctionPointerForDelegate<SetValueDelegate>(SetValueForwarding));

            BurstObjectNotify.SetListValue.Data = new FunctionPointer<SetListValueDelegate>(
                Marshal.GetFunctionPointerForDelegate<SetListValueDelegate>(SetValueForwarding));

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
#endif