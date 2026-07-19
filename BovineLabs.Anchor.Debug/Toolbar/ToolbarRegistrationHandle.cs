// <copyright file="ToolbarRegistrationHandle.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;

    /// <summary>
    /// Opaque unmanaged handle for a durable toolbar registration.
    /// </summary>
    public readonly struct ToolbarRegistrationHandle : IEquatable<ToolbarRegistrationHandle>
    {
        private readonly long ownerId;
        private readonly int registrationId;

        internal ToolbarRegistrationHandle(long ownerId, int registrationId)
        {
            this.ownerId = ownerId;
            this.registrationId = registrationId;
        }

        internal long OwnerId => this.ownerId;

        internal int RegistrationId => this.registrationId;

        /// <summary>Gets a value indicating whether this handle identifies a registration.</summary>
        public bool IsValid => this.ownerId != 0 && this.registrationId != 0;

        /// <inheritdoc />
        public bool Equals(ToolbarRegistrationHandle other)
        {
            return this.ownerId == other.ownerId && this.registrationId == other.registrationId;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ToolbarRegistrationHandle other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ownerId, this.registrationId);
        }

        public static bool operator ==(ToolbarRegistrationHandle left, ToolbarRegistrationHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ToolbarRegistrationHandle left, ToolbarRegistrationHandle right)
        {
            return !left.Equals(right);
        }
    }
}
