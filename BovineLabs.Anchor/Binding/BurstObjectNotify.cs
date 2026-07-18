// <copyright file="BurstObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core.Utility;
    using Unity.Assertions;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Scripting.LifecycleManagement;

    internal unsafe struct SetValueParams
    {
        public IntPtr Target;
        public FixedString64Bytes Property;
        public void* Field;
        public void* NewValue;
        public int Length;
    }

    internal unsafe struct SetListValueParams
    {
        public IntPtr Target;
        public FixedString64Bytes Property;
        public UntypedUnsafeList* Field;
        public void* NewValues;
        public int Length;
        public int ElementSize;
    }

    internal struct NotifyParams
    {
        public IntPtr Target;
        public FixedString64Bytes Property;
    }

    internal static unsafe partial class BurstObjectNotify
    {
        internal static readonly Dictionary<IntPtr, IBindingObjectNotify> Changed = new();

        [OnCodeInitializing]
        private static void InitializeTrampolines()
        {
            Burst.SetValue.Data = new BurstTrampoline(&SetValueForwarding);
            Burst.SetListValue.Data = new BurstTrampoline(&SetListValueForwarding);
            Burst.Notify.Data = new BurstTrampoline(&NotifyForwarding);
        }

        private static void SetValueForwarding(void* argumentsPtr, int argumentsSize)
        {
            ref var handle = ref BurstTrampoline.ArgumentsFromPtr<SetValueParams>(argumentsPtr, argumentsSize);

            if (Changed.TryGetValue(handle.Target, out var notify))
            {
                notify.OnPropertyChanging(handle.Property);
                UnsafeUtility.MemCpy(handle.Field, handle.NewValue, handle.Length);
                notify.OnPropertyChanged(handle.Property);
            }
            else
            {
                UnsafeUtility.MemCpy(handle.Field, handle.NewValue, handle.Length);
            }
        }

        private static void SetListValueForwarding(void* argumentsPtr, int argumentsSize)
        {
            ref var handle = ref BurstTrampoline.ArgumentsFromPtr<SetListValueParams>(argumentsPtr, argumentsSize);

            if (Changed.TryGetValue(handle.Target, out var notify))
            {
                notify.OnPropertyChanging(handle.Property);
                WriteValue(ref handle);
                notify.OnPropertyChanged(handle.Property);
            }
            else
            {
                WriteValue(ref handle);
            }

            return;

            void WriteValue(ref SetListValueParams handle)
            {
                Assert.IsTrue(handle.Field != handle.NewValues, "Trying to write self to destination");

                handle.Field->Resize(handle.Length, handle.ElementSize);
                UnsafeUtility.MemCpy(handle.Field->Ptr, handle.NewValues, handle.Length * handle.ElementSize);
            }
        }

        private static void NotifyForwarding(void* argumentsPtr, int argumentsSize)
        {
            ref var handle = ref BurstTrampoline.ArgumentsFromPtr<NotifyParams>(argumentsPtr, argumentsSize);

            if (Changed.TryGetValue(handle.Target, out var notify))
            {
                notify.OnPropertyChanged(handle.Property);
            }
        }

        internal static class Burst
        {
            public static readonly SharedStatic<BurstTrampoline> SetValue = SharedStatic<BurstTrampoline>.GetOrCreate<IBindingObjectNotify, SetValueParams>();
            public static readonly SharedStatic<BurstTrampoline> SetListValue = SharedStatic<BurstTrampoline>.GetOrCreate<IBindingObjectNotify, SetListValueParams>();
            public static readonly SharedStatic<BurstTrampoline> Notify = SharedStatic<BurstTrampoline>.GetOrCreate<IBindingObjectNotify, NotifyParams>();
        }
    }
}