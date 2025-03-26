// <copyright file="BindingObjectNotifyDataExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Runtime.CompilerServices;
    using Unity.Assertions;
    using Unity.Burst.CompilerServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public static unsafe class BindingObjectNotifyDataExtensions
    {
        public static bool SetProperty<T, TV>(this ref T binding, ref TV field, TV newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged, IEquatable<TV>
        {
            if (field.Equals(newValue))
            {
                return false;
            }

            SetValueNotify(ref binding, ref field, newValue, propertyName);
            return true;
        }

        public static bool SetProperty<T, TV>(this ref T binding, ref Changed<TV> field, Changed<TV> newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged, IEquatable<TV>
        {
            if (field.Value.Equals(newValue.Value))
            {
                // This is a reset operation
                if (!newValue.HasChanged)
                {
                    field = new Changed<TV>(field.Value, false);
                }

                return false;
            }

            SetValueNotify(ref binding, ref field, newValue, propertyName);

            return true;
        }

        public static bool SetProperty<T, TV>(
            this ref T binding, NativeList<TV> field, MultiContainer<TV> newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged, IEquatable<TV>
        {
            if (!field.IsCreated)
            {
                return false;
            }

            newValue.ThrowContainersMatch(field);

            if (newValue.ArraysEqual(field.AsArray()))
            {
                return false;
            }

            var data = newValue.GetAsTempArray();
            SetValueNotify(ref binding, field, (TV*)data.Ptr, data.Length, propertyName);
            return true;
        }

        // TODO
        public static bool SetProperty<T, TV>(
            this ref T binding, ref ChangedList<TV> field, ChangedList<TV> newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged, IEquatable<TV>
        {
            if (field.Value.IsCreated == newValue.Value.IsCreated)
            {
                // Both lists aren't created
                if (!field.Value.IsCreated)
                {
                    return false;
                }

                if (field.Value.GetUnsafeReadOnlyPtr() == newValue.Value.GetUnsafeReadOnlyPtr())
                {
                    // Same list, only need to notify
                    binding.Notify(propertyName);
                    return true;
                }
            }

            // Different lists, need to write
            SetValueNotify(ref binding, ref field, newValue, propertyName);
            return true;
        }

        public static void SetValueNotify<T, TV>(this ref T binding, ref TV field, TV newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged
        {
            if (Hint.Likely(BurstObjectNotify.SetValue.Data.IsCreated))
            {
                var target = (IntPtr)UnsafeUtility.AddressOf(ref binding);
                var fieldPtr = UnsafeUtility.AddressOf(ref field);
                var valuePtr = &newValue;

                BurstObjectNotify.SetValue.Data.Invoke(target, propertyName, fieldPtr, valuePtr, sizeof(TV));
            }
            else
            {
                field = newValue;
            }
        }

        public static void SetValueNotify<T, TV>(
            this ref T binding, NativeList<TV> field, TV* newValue, int length, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged
        {
            if (Hint.Likely(BurstObjectNotify.SetListValue.Data.IsCreated))
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.IsTrue(field.GetUnsafePtr() != null); // This is mostly just doing a write safety check
#endif

                var target = (IntPtr)UnsafeUtility.AddressOf(ref binding);
                var unsafeList = (UntypedUnsafeList*)field.GetUnsafeList();

                BurstObjectNotify.SetListValue.Data.Invoke(target, propertyName, unsafeList, newValue, length, sizeof(TV));
            }
            else
            {
                field.Clear();
                field.AddRange(newValue, length);
            }
        }

        public static void Notify<T>(this ref T binding, FixedString64Bytes propertyName)
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
