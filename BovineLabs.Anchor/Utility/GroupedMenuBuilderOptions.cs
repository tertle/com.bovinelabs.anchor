// <copyright file="GroupedMenuBuilderOptions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    /// <summary> Options for grouped AppUI menu construction. </summary>
    public struct GroupedMenuBuilderOptions
    {
        /// <summary> Default maximum number of entries shown at one menu level. </summary>
        public const int DefaultMaxItemsPerMenu = 12;

        /// <summary> Default maximum normalized label prefix length used for recursive grouping. </summary>
        public const int DefaultMaxPrefixLength = 8;

        private int maxItemsPerMenu;
        private int maxPrefixLength;
        private bool keepSingleItemGroups;

        /// <summary> Gets or sets the maximum number of visible actions or submenu groups in one menu level. </summary>
        public int MaxItemsPerMenu
        {
            readonly get => this.maxItemsPerMenu <= 0 ? DefaultMaxItemsPerMenu : this.maxItemsPerMenu;
            set => this.maxItemsPerMenu = value;
        }

        /// <summary> Gets or sets the maximum normalized label prefix length used when recursively splitting large groups. </summary>
        public int MaxPrefixLength
        {
            readonly get => this.maxPrefixLength <= 0 ? DefaultMaxPrefixLength : this.maxPrefixLength;
            set => this.maxPrefixLength = value;
        }

        /// <summary> Gets or sets a value indicating whether groups containing a single item should be added as direct actions. </summary>
        public bool FlattenSingleItemGroups
        {
            readonly get => !this.keepSingleItemGroups;
            set => this.keepSingleItemGroups = !value;
        }

        internal readonly int EffectiveMaxItemsPerMenu => this.MaxItemsPerMenu < 2 ? 2 : this.MaxItemsPerMenu;

        internal readonly int EffectiveMaxPrefixLength => this.MaxPrefixLength < 1 ? 1 : this.MaxPrefixLength;
    }
}
