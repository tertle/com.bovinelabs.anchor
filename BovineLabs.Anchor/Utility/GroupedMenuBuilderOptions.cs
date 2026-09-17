namespace BovineLabs.Anchor
{
    public struct GroupedMenuBuilderOptions
    {
        public const int DefaultMaxItemsPerMenu = 12;

        public const int DefaultMaxPrefixLength = 8;

        private int maxItemsPerMenu;
        private int maxPrefixLength;
        private bool keepSingleItemGroups;

        public int MaxItemsPerMenu
        {
            readonly get => this.maxItemsPerMenu <= 0 ? DefaultMaxItemsPerMenu : this.maxItemsPerMenu;
            set => this.maxItemsPerMenu = value;
        }

        public int MaxPrefixLength
        {
            readonly get => this.maxPrefixLength <= 0 ? DefaultMaxPrefixLength : this.maxPrefixLength;
            set => this.maxPrefixLength = value;
        }

        public bool FlattenSingleItemGroups
        {
            readonly get => !this.keepSingleItemGroups;
            set => this.keepSingleItemGroups = !value;
        }

        internal readonly int EffectiveMaxItemsPerMenu => this.MaxItemsPerMenu < 2 ? 2 : this.MaxItemsPerMenu;

        internal readonly int EffectiveMaxPrefixLength => this.MaxPrefixLength < 1 ? 1 : this.MaxPrefixLength;
    }
}
