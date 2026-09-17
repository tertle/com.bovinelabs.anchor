namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class AnchorNavArgument : IEquatable<AnchorNavArgument>
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string value;

        public AnchorNavArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name => this.name;

        public string Value => this.value;

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

            return this.name == other.name &&
                   this.value == other.value;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as AnchorNavArgument);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.name != null ? this.name.GetHashCode() : 0;
                return (hashCode * 397) ^ (this.value != null ? this.value.GetHashCode() : 0);
            }
        }
    }
}
