// <copyright file="Utility.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Core.Utility;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    internal static class Utility
    {
        /// <summary> Searches all assemblies to find all types that implement a type. </summary>
        /// <typeparam name="T"> The base type that is inherited from. </typeparam>
        /// <returns> All the types. </returns>
        public static IEnumerable<Type> GetAllImplementations<T>()
            where T : class
        {
            var type = typeof(T);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type)
                .Where(t => !t.IsInterface && !t.IsAbstract)
                .Where(t => type.IsAssignableFrom(t));
        }

        /// <summary> Searches all assemblies to find all types that have an attribute. </summary>
        /// <typeparam name="T"> The attribute to search for. </typeparam>
        /// <returns> All the types. </returns>
        public static IEnumerable<Type> GetAllWithAttribute<T>()
            where T : Attribute
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t.GetCustomAttribute<T>() != null);
        }

        public static void AddRangeNative<T>(this List<T> list, NativeArray<T> array)
            where T : struct
        {
            AddRangeNative(list, array, array.Length);
        }

        public static unsafe void AddRangeNative<T>(this List<T> list, NativeArray<T> array, int length)
            where T : struct
        {
            list.AddRangeNative(array.GetUnsafeReadOnlyPtr(), length);
        }

        public static unsafe void AddRangeNative<T>(this List<T> list, void* arrayBuffer, int length)
            where T : struct
        {
            if (length == 0)
            {
                return;
            }

            var index = list.Count;
            var newLength = index + length;

            // Resize our list if we require
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }

            var items = NoAllocHelpers.ExtractArrayFromList(list);
            var size = UnsafeUtility.SizeOf<T>();

            // Get the pointer to the end of the list
            var bufferStart = (IntPtr)UnsafeUtility.AddressOf(ref items[0]);
            var buffer = (byte*)(bufferStart + (size * index));

            UnsafeUtility.MemCpy(buffer, arrayBuffer, length * (long)size);

            NoAllocHelpers.ResizeList(list, newLength);
        }
    }
}
