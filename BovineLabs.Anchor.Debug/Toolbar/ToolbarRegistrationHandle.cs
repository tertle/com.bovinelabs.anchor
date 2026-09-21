namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;

    public readonly struct ToolbarRegistrationHandle : IEquatable<ToolbarRegistrationHandle>
    {
        private readonly long _ownerId;
        private readonly int _registrationId;

        internal ToolbarRegistrationHandle(long ownerId, int registrationId)
        {
            _ownerId = ownerId;
            _registrationId = registrationId;
        }

        internal long OwnerId => _ownerId;

        internal int RegistrationId => _registrationId;

        public bool IsValid => _ownerId != 0 && _registrationId != 0;

        public bool Equals(ToolbarRegistrationHandle other)
        {
            return _ownerId == other._ownerId && _registrationId == other._registrationId;
        }

        public override bool Equals(object obj)
        {
            return obj is ToolbarRegistrationHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_ownerId ^ (int)(_ownerId >> 32);
                return (hashCode * 397) ^ _registrationId;
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
