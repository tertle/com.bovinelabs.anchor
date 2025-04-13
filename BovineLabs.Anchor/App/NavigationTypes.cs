// // <copyright file="NavigationTypes.cs" company="BovineLabs">
// //     Copyright (c) BovineLabs. All rights reserved.
// // </copyright>
//
// #if BL_CORE
// namespace BovineLabs.Anchor
// {
//     using Unity.Burst;
//     using Unity.Collections;
//     using Unity.Collections.LowLevel.Unsafe;
//     using Unity.Entities;
//
//     public static class NavigationTypes
//     {
//         internal static readonly SharedStatic<UnsafeHashMap<FixedString64Bytes, ComponentType>> Map =
//             SharedStatic<UnsafeHashMap<FixedString64Bytes, ComponentType>>.GetOrCreate<AnchorTypeSettings.NavigationComponent>();
//
//         public static ComponentType Get(FixedString64Bytes name)
//         {
//             Map.Data.TryGetValue(name, out var componentType);
//
//             return componentType;
//         }
//     }
// }
// #endif


