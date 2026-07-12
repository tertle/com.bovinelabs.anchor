// <copyright file="IBindingObjectNotify`T.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary> The interfacing for supporting binding and your view model from burst. Should only be applied to structs. </summary>
    /// <typeparam name="T"> The type of the inherited struct. </typeparam>
    public interface IBindingObjectNotify<T> : IBindingObjectNotify
        where T : unmanaged
    {
        /// <summary>Gets a reference to the unmanaged data backing the binding.</summary>
        ref T Value { get; }

        void Pin();

        void Unpin();
    }

    internal static unsafe class IBindingObjectExtensions
    {
        public static T* PinObject<T>(this IBindingObjectNotify<T> bindingObjectNotify)
            where T : unmanaged
        {
            bindingObjectNotify.Pin();
            BurstObjectNotify.Changed[(IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value)] = bindingObjectNotify;
            return (T*)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value);
        }

        public static void UnpinObject<T>(this IBindingObjectNotify<T> bindingObjectNotify)
            where T : unmanaged
        {
            BurstObjectNotify.Changed.Remove((IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value));
            bindingObjectNotify.Unpin();
        }
    }
}
