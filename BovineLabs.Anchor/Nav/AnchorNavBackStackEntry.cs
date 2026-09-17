namespace BovineLabs.Anchor.Nav
{
    using System;

    internal sealed class AnchorNavBackStackEntry
    {
        internal AnchorNavBackStackEntry(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, AnchorNavStackSnapshot snapshot = null)
        {
            this.Destination = destination;
            this.Options = options ?? new AnchorNavOptions();
            this.Arguments = arguments ?? Array.Empty<AnchorNavArgument>();
            this.Snapshot = snapshot ?? AnchorNavStackSnapshot.Empty;
        }

        public string Destination { get; }

        public AnchorNavOptions Options { get; }

        public AnchorNavArgument[] Arguments { get; }

        public AnchorNavStackSnapshot Snapshot { get; }
    }
}
