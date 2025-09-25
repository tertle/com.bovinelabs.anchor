// <copyright file="AnchorNavActionAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;

    /// <summary>
    /// Attribute used to mark members that describe navigation actions which should
    /// automatically be registered with <see cref="AnchorNavHost"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AnchorNavActionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavActionAttribute"/> class.
        /// </summary>
        /// <param name="name">The unique action name. If null, the member name is used.</param>
        public AnchorNavActionAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Action name cannot be null or whitespace.", nameof(name));
            }

            this.Name = name;
        }

        /// <summary>Gets the action name.</summary>
        public string Name { get; }
    }
}
