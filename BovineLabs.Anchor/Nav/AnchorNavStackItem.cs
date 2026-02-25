// <copyright file="AnchorNavStackItem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Linq;

    internal sealed class AnchorNavStackItem
    {
        public AnchorNavStackItem(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, bool isPopup)
        {
            this.Destination = destination;
            this.Options = options ?? new AnchorNavOptions();
            this.Arguments = arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>();
            this.IsPopup = isPopup;
        }

        public string Destination { get; }

        public AnchorNavOptions Options { get; }

        public AnchorNavArgument[] Arguments { get; }

        public bool IsPopup { get; }
    }
}
