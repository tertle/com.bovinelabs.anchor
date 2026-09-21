namespace BovineLabs.Anchor
{
    public struct GroupedMenuBuilderOptions
    {
        public const int DefaultMaxItemsPerMenu = 12;

        public const int DefaultMaxPrefixLength = 8;

        private int _maxItemsPerMenu;
        private int _maxPrefixLength;
        private bool _keepSingleItemGroups;

        public int MaxItemsPerMenu
        {
            readonly get => _maxItemsPerMenu <= 0 ? DefaultMaxItemsPerMenu : _maxItemsPerMenu;
            set => _maxItemsPerMenu = value;
        }

        public int MaxPrefixLength
        {
            readonly get => _maxPrefixLength <= 0 ? DefaultMaxPrefixLength : _maxPrefixLength;
            set => _maxPrefixLength = value;
        }

        public bool FlattenSingleItemGroups
        {
            readonly get => !_keepSingleItemGroups;
            set => _keepSingleItemGroups = !value;
        }

        internal readonly int EffectiveMaxItemsPerMenu => MaxItemsPerMenu < 2 ? 2 : MaxItemsPerMenu;

        internal readonly int EffectiveMaxPrefixLength => MaxPrefixLength < 1 ? 1 : MaxPrefixLength;
    }
}
