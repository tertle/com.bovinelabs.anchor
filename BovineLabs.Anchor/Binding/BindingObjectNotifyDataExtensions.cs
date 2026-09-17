namespace BovineLabs.Anchor.Binding
{
    using System;
    using System.Runtime.CompilerServices;
    using BovineLabs.Core.Assertions;
    using BovineLabs.Core.Utility;
    using Unity.Assertions;
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

        /// <summary>
        /// Notifies even when the underlying list instance is unchanged.
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
        /// Emits notifications without checking equality.
        /// </summary>
        public static void SetValueNotify<T, TV>(this ref T binding, ref TV field, TV newValue, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged
        {
            Check.Assume(BurstObjectNotify.Burst.SetValue.Data.IsCreated);

            BurstObjectNotify.Burst.SetValue.Data.Invoke(new SetValueParams
            {
                Target = (IntPtr)UnsafeUtility.AddressOf(ref binding),
                Property = propertyName,
                Field = UnsafeUtility.AddressOf(ref field),
                NewValue = &newValue,
                Length = sizeof(TV),
            });
        }

        public static void SetValueNotify<T, TV>(
            this ref T binding, NativeList<TV> field, TV* newValue, int length, [CallerMemberName] string propertyName = "")
            where T : unmanaged
            where TV : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(field.GetUnsafePtr() != null); // This is mostly just doing a write safety check
#endif

            Check.Assume(BurstObjectNotify.Burst.SetListValue.Data.IsCreated);

            BurstObjectNotify.Burst.SetListValue.Data.Invoke(new SetListValueParams
            {
                Target = (IntPtr)UnsafeUtility.AddressOf(ref binding),
                Property = propertyName,
                Field = (UntypedUnsafeList*)field.GetUnsafeList(),
                NewValues = newValue,
                Length = length,
                ElementSize = sizeof(TV),
                ElementAlignment = UnsafeUtility.AlignOf<TV>(),
            });
        }

        public static void Notify<T>(this ref T binding, FixedString64Bytes propertyName)
            where T : unmanaged
        {
            Check.Assume(BurstObjectNotify.Burst.Notify.Data.IsCreated);

            var target = (IntPtr)UnsafeUtility.AddressOf(ref binding);
            BurstObjectNotify.Burst.Notify.Data.Invoke(new NotifyParams
            {
                Target = target,
                Property = propertyName,
            });
        }
    }
}
