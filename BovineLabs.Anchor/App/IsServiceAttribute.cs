// <copyright file="IsServiceAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Indicates that a non-visual class or interface is a service and should be automatically registered with the Anchor service container.
    /// </summary>
    /// <remarks><see cref="UnityEngine.UIElements.VisualElement"/> implementations are rejected by the service collection.</remarks>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IsServiceAttribute : Attribute
    {
    }
}
