// <copyright file="SystemPropertyAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class SystemPropertyAttribute : Attribute
    {
    }
}
#endif
