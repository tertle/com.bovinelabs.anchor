// <copyright file="Core.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class Core
    {
        /// <summary> Searches all assemblies to find all types that implement a type. </summary>
        /// <typeparam name="T"> The base type that is inherited from. </typeparam>
        /// <returns> All the types. </returns>
        public static IEnumerable<Type> GetAllImplementations<T>()
            where T : class
        {
            var type = typeof(T);

            return AppDomain
                .CurrentDomain
                .GetAssemblies()
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
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(t => t.GetCustomAttribute<T>() != null);
        }
    }
}
