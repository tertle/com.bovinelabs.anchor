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

        internal static unsafe void Load(IBindingObjectNotify<T> bindingObjectNotify)
        {
            BurstUIInterop.Changed[(IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value)] = bindingObjectNotify;
        }

        internal static unsafe void Unload(IBindingObjectNotify<T> bindingObjectNotify)
        {
            BurstUIInterop.Changed.Remove((IntPtr)UnsafeUtility.AddressOf(ref bindingObjectNotify.Value));
        }
    }

    internal static class IBindingObjectExtensions
    {
        public static void Load<T>(this IBindingObjectNotify<T> bindingObjectNotify)
            where T : unmanaged
        {
            IBindingObjectNotify<T>.Load(bindingObjectNotify);
        }

        public static void Unload<T>(this IBindingObjectNotify<T> bindingObjectNotify)
            where T : unmanaged
        {
            IBindingObjectNotify<T>.Unload(bindingObjectNotify);
        }
    }
}
