namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Linq;

    internal sealed class AnchorNavStackItem
    {
        public AnchorNavStackItem(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, bool isPopup)
        {
            Destination = destination;
            Options = options ?? new AnchorNavOptions();
            Arguments = arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>();
            IsPopup = isPopup;
        }

        public string Destination { get; }

        public AnchorNavOptions Options { get; }

        public AnchorNavArgument[] Arguments { get; }

        public bool IsPopup { get; }
    }
}
