// <copyright file="AnchorNavActionAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;

    /// <summary>
    /// Attribute used to mark static parameterless methods that return navigation actions for
    /// automatic registration with <see cref="AnchorNavHost"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class AnchorNavActionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavActionAttribute"/> class.
        /// </summary>
        /// <param name="name">The required unique action name.</param>
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
