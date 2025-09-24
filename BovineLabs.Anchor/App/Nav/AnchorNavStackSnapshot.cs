// <copyright file="AnchorNavStackSnapshot.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class AnchorNavStackSnapshot
    {
        private static readonly AnchorNavStackSnapshot EmptyInstance = new(Array.Empty<AnchorNavStackItem>());

        private readonly List<AnchorNavStackItem> items;

        public AnchorNavStackSnapshot(IEnumerable<AnchorNavStackItem> items)
        {
            this.items = new List<AnchorNavStackItem>(items ?? Array.Empty<AnchorNavStackItem>());
        }

        public static AnchorNavStackSnapshot Empty => EmptyInstance;

        public IReadOnlyList<AnchorNavStackItem> Items => this.items;

        public AnchorNavStackItem Top => this.items.Count > 0 ? this.items[^1] : null;

        public bool HasPopups => this.items.Any(i => i.IsPopup);

        public AnchorNavStackSnapshot WithoutPopups()
        {
            if (!this.HasPopups)
            {
                return this;
            }

            var filtered = new List<AnchorNavStackItem>(this.items.Count);
            foreach (var item in this.items)
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
