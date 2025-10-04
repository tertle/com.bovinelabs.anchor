// <copyright file="AnchorNavStackItem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Linq;
    using Unity.AppUI.Navigation;

    internal sealed class AnchorNavStackItem
    {
        public AnchorNavStackItem(string destination, AnchorNavOptions options, Argument[] arguments, bool isPopup)
        {
            this.Destination = destination;
            this.Options = options ?? new AnchorNavOptions();
            this.Arguments = arguments?.ToArray() ?? Array.Empty<Argument>();
            this.IsPopup = isPopup;
        }

        public string Destination { get; }

        public AnchorNavOptions Options { get; }

        public Argument[] Arguments { get; }

        public bool IsPopup { get; }
    }
}
