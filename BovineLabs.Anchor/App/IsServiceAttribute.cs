// <copyright file="IsServiceAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IsServiceAttribute : Attribute
    {
    }
}
