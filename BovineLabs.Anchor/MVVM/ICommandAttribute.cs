// <copyright file="ICommandAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    /// <summary>
    /// Marks a method for generated relay-command output.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ICommandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets an optional method name that controls command can-execute.
        /// </summary>
        public string CanExecuteMethod { get; set; }
    }
}
