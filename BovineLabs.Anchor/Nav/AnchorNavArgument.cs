namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class AnchorNavArgument : IEquatable<AnchorNavArgument>
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private string _value;

        public AnchorNavArgument(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public string Name => _name;

        public string Value => _value;

        public static AnchorNavArgument String(string name, string value)
        {
            return new AnchorNavArgument(name, value);
        }

        public static AnchorNavArgument From(string name, string value)
        {
            return new AnchorNavArgument(name, value);
        }

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

            return _name == other._name &&
                   _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AnchorNavArgument);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _name != null ? _name.GetHashCode() : 0;
                return (hashCode * 397) ^ (_value != null ? _value.GetHashCode() : 0);
            }
        }
    }
}
