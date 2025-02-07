// <copyright file="Core.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_CORE
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
            var coreAssembly = type.Assembly;

            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(asm => asm.IsAssemblyReferencingAssembly(coreAssembly))
                .SelectMany(asm => asm.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract && !t.ContainsGenericParameters).Where(type.IsAssignableFrom));
        }

        /// <summary> Searches all assemblies to find all types that have an attribute. </summary>
        /// <typeparam name="T"> The attribute to search for. </typeparam>
        /// <returns> All the types. </returns>
        public static IEnumerable<Type> GetAllWithAttribute<T>()
            where T : Attribute
        {
            var attributeType = typeof(T);
            var coreAssembly = attributeType.Assembly;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsAssemblyReferencingAssembly(coreAssembly))
                {
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsDefined(attributeType, false))
                    {
                        continue;
                    }

                    yield return type;
                }
            }
        }

        private static bool IsAssemblyReferencingAssembly(this Assembly assembly, Assembly reference)
        {
            if (assembly == reference)
            {
                return true;
            }

            var referenceName = reference.GetName().Name;
            return assembly.GetReferencedAssemblies().Any(referenced => referenced.Name == referenceName);
        }
    }
}
#endif
