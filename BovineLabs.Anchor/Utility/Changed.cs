namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    [Serializable]
    public readonly struct Changed<T>
        where T : unmanaged
    {
        public readonly T Value;

        // Used by source generators
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public readonly bool HasChanged;

        public Changed(T value, bool hasChanged = true)
        {
            this.Value = value;
            this.HasChanged = hasChanged;
        }

        /// <summary>
        /// Conversion marks the value as changed.
        /// </summary>
        public static implicit operator Changed<T>(T value)
        {
            return new Changed<T>(value);
        }
    }
}
