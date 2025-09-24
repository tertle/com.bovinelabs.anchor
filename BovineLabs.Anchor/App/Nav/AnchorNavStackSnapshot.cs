// <copyright file="AnchorNavStackSnapshot.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;

    internal sealed class AnchorNavStackSnapshot
    {
        private static readonly AnchorNavStackSnapshot EmptyInstance = new(Array.Empty<AnchorNavStackItem>());

        private readonly IReadOnlyList<AnchorNavStackItem> items;

        public AnchorNavStackSnapshot(IEnumerable<AnchorNavStackItem> items)
        {
            this.items = new List<AnchorNavStackItem>(items ?? Array.Empty<AnchorNavStackItem>());
        }

        public static AnchorNavStackSnapshot Empty => EmptyInstance;

        public IReadOnlyList<AnchorNavStackItem> Items => this.items;

        public AnchorNavStackItem Top => this.items.Count > 0 ? this.items[^1] : null;
    }
}
