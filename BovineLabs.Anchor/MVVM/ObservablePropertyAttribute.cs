// <copyright file="ObservablePropertyAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    /// <summary>
    /// Marks a field for generated observable property output.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ObservablePropertyAttribute : Attribute
    {
    }

    /// <summary>
    /// Adds additional changed notifications for generated observable properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class AlsoNotifyChangeForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlsoNotifyChangeForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The dependent property name.</param>
        public AlsoNotifyChangeForAttribute(string propertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlsoNotifyChangeForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The first dependent property name.</param>
        /// <param name="propertyNames">Additional dependent property names.</param>
        public AlsoNotifyChangeForAttribute(string propertyName, params string[] propertyNames)
        {
        }
    }
}
