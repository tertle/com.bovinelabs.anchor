// <copyright file="BurstObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System;
    using Unity.Burst;
    using Unity.Collections;

    internal unsafe delegate void SetValueDelegate(IntPtr target, in FixedString64Bytes property, void* field, void* newValue, int length);

    internal unsafe delegate void SetListValueDelegate(
        IntPtr target, in FixedString64Bytes property, UntypedUnsafeList* field, void* newValues, int length, int elementSize);

    internal delegate void NotifyDelegate(IntPtr target, in FixedString64Bytes property);

    /// <summary> Function pointers for forwarding burst code to managed UI. </summary>
    /// <remarks>
    /// Initialized in but separated from <see cref="BurstUIInterop"/> to avoid burst bringing in static managed objects, even if not used.
    /// </remarks>
    internal static class BurstObjectNotify
    {
        public static readonly SharedStatic<FunctionPointer<SetValueDelegate>> SetValue =
            SharedStatic<FunctionPointer<SetValueDelegate>>.GetOrCreate<FunctionPointer<SetValueDelegate>>();

        public static readonly SharedStatic<FunctionPointer<SetListValueDelegate>> SetListValue =
            SharedStatic<FunctionPointer<SetListValueDelegate>>.GetOrCreate<FunctionPointer<SetListValueDelegate>>();

        public static readonly SharedStatic<FunctionPointer<NotifyDelegate>> Notify =
            SharedStatic<FunctionPointer<NotifyDelegate>>.GetOrCreate<FunctionPointer<NotifyDelegate>>();
    }
}