namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;

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

        public bool IsValid => this.ownerId != 0 && this.registrationId != 0;

        public bool Equals(ToolbarRegistrationHandle other)
        {
            return this.ownerId == other.ownerId && this.registrationId == other.registrationId;
        }

        public override bool Equals(object obj)
        {
            return obj is ToolbarRegistrationHandle other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)this.ownerId ^ (int)(this.ownerId >> 32);
                return (hashCode * 397) ^ this.registrationId;
            }
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
