// <copyright file="BindingObjectNotifyDataExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Runtime.CompilerServices;
    using Unity.Assertions;
    using Unity.Burst.CompilerServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Helpers that streamline updating unmanaged view model data while emitting binding notifications.
    /// </summary>
    public static unsafe class BindingObjectNotifyDataExtensions
    {
        /// <summary>
        /// Updates a field when the value changes and raises property changing/changed events.
        /// </summary>
        /// <typeparam name="T">The unmanaged binding struct.</typeparam>
        /// <typeparam name="TV">The value type being tracked.</typeparam>
        /// <param name="binding">The binding to notify.</param>
        /// <param name="field">The backing field reference.</param>
        /// <param name="newValue">The candidate value.</param>
        /// <param name="propertyName">The property name to notify.</param>
        /// <returns>True when the value changed and notifications were sent.</returns>
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

        /// <summary>
        /// Updates a <see cref="Changed{T}"/> wrapper while ensuring only real changes trigger notifications.
        /// </summary>
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

        /// <summary>
        /// Copies a container of values into a native list and notifies when data differs.
        /// </summary>
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
        /// <summary>
        /// Synchronizes a <see cref="ChangedList{T}"/> and notifies consumers even when the underlying list instance matches.
        /// </summary>
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

        /// <summary>
        /// Writes a new value to the backing field and emits notifications without equality checks.
        /// </summary>
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

        /// <summary>
        /// Writes a block of values into a native list and notifies binding listeners.
        /// </summary>
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

        /// <summary>
        /// Raises a change notification for a property without mutating data.
        /// </summary>
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
