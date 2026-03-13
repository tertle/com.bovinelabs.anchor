// <copyright file="AnchorNavActiveEntry.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;

    internal sealed class AnchorNavActiveEntry
    {
        public AnchorNavActiveEntry(
            string destination,
            AnchorNavArgument[] arguments,
            bool isPopup,
            AnchorNavOptions options,
            VisualElement element)
        {
            this.Destination = destination;
            this.Arguments = arguments ?? Array.Empty<AnchorNavArgument>();
            this.IsPopup = isPopup;
            this.Options = options ?? new AnchorNavOptions();
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public string Destination { get; }

        public bool IsPopup { get; }

        public AnchorNavOptions Options { get; private set; }

        public AnchorNavArgument[] Arguments { get; private set; }

        public VisualElement Element { get; }

        public void Update(AnchorNavStackItem item)
        {
            this.Options = item.Options;
            this.Arguments = item.Arguments;
        }
    }
}
