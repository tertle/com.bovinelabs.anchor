// <copyright file="AnchorNavArgument.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Serialized navigation argument passed between Anchor destinations.
    /// </summary>
    [Serializable]
    public sealed class AnchorNavArgument : IEquatable<AnchorNavArgument>
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavArgument"/> class.
        /// </summary>
        /// <param name="name">Argument key.</param>
        /// <param name="value">Argument value.</param>
        public AnchorNavArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>Gets the argument key.</summary>
        public string Name => this.name;

        /// <summary>Gets the argument value.</summary>
        public string Value => this.value;

        /// <summary>Create a string navigation argument.</summary>
        /// <param name="name">Argument key.</param>
        /// <param name="value">Argument value.</param>
        /// <returns>Created argument.</returns>
        public static AnchorNavArgument String(string name, string value)
        {
            return new AnchorNavArgument(name, value);
        }

        /// <summary>Create a string navigation argument.</summary>
        /// <param name="name">Argument key.</param>
        /// <param name="value">Argument value.</param>
        /// <returns>Created argument.</returns>
        public static AnchorNavArgument From(string name, string value)
        {
            return new AnchorNavArgument(name, value);
        }

        /// <inheritdoc />
        public bool Equals(AnchorNavArgument other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return this.name == other.name &&
                   this.value == other.value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as AnchorNavArgument);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.name,
                this.value);
        }
    }
}
