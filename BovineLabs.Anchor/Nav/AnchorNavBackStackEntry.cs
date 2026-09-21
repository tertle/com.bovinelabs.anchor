namespace BovineLabs.Anchor.Nav
{
    using System;

    internal sealed class AnchorNavBackStackEntry
    {
        internal AnchorNavBackStackEntry(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, AnchorNavStackSnapshot snapshot = null)
        {
            Destination = destination;
            Options = options ?? new AnchorNavOptions();
            Arguments = arguments ?? Array.Empty<AnchorNavArgument>();
            Snapshot = snapshot ?? AnchorNavStackSnapshot.Empty;
        }

        public string Destination { get; }

        public AnchorNavOptions Options { get; }

        public AnchorNavArgument[] Arguments { get; }

        public AnchorNavStackSnapshot Snapshot { get; }
    }
}
