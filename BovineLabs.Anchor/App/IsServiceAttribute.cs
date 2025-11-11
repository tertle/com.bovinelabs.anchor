// <copyright file="IsServiceAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Indicates that a class or interface is a service and should be automatically registered with the Anchor service container.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IsServiceAttribute : Attribute
    {
    }
}
