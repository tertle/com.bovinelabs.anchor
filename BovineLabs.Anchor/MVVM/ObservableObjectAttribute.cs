// <copyright file="ObservableObjectAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    /// <summary>
    /// Marks a partial class as a MVVM observable object generation target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ObservableObjectAttribute : Attribute
    {
    }
}
