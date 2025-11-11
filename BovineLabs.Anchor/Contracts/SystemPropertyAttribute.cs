// <copyright file="SystemPropertyAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;

    /// <summary>
    /// Marks a DOTS field so it can be auto-populated via reflection when binding systems to UI state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SystemPropertyAttribute : Attribute
    {
    }
}
