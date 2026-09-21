namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Scripting.LifecycleManagement;

    internal sealed class AnchorNavStackSnapshot
    {
        [NoAutoStaticsCleanup]
        private static readonly AnchorNavStackSnapshot EmptyInstance = new(Array.Empty<AnchorNavStackItem>());

        private readonly List<AnchorNavStackItem> _items;

        public AnchorNavStackSnapshot(IEnumerable<AnchorNavStackItem> items)
        {
            _items = new List<AnchorNavStackItem>(items ?? Array.Empty<AnchorNavStackItem>());
        }

        public static AnchorNavStackSnapshot Empty => EmptyInstance;

        public IReadOnlyList<AnchorNavStackItem> Items => _items;

        public AnchorNavStackItem Top => _items.Count > 0 ? _items[^1] : null;

        public bool HasPopups => _items.Any(i => i.IsPopup);

        public AnchorNavStackSnapshot WithoutPopups()
        {
            if (!HasPopups)
            {
                return this;
            }

            var filtered = new List<AnchorNavStackItem>(_items.Count);
            foreach (var item in _items)
            {
                if (!item.IsPopup)
                {
                    filtered.Add(item);
                }
            }

            return new AnchorNavStackSnapshot(filtered);
        }
    }
}
