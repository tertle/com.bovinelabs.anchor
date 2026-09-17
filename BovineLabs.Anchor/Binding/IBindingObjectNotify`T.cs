namespace BovineLabs.Anchor.Binding
{
    using System;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Implement on structs only; T is the implementing struct.
    /// </summary>
    public interface IBindingObjectNotify<T> : IBindingObjectNotify
        where T : unmanaged
    {
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
