namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class AnchorNavHostSaveState
    {
        public AnchorNavHostSaveState(
            string currentDestination, AnchorNavAnimation currentPopEnterAnimation, AnchorNavAnimation currentPopExitAnimation,
            IReadOnlyList<StackItem> activeStack, IReadOnlyList<BackStackEntry> backStack)
        {
            this.CurrentDestination = currentDestination;
            this.CurrentPopEnterAnimation = currentPopEnterAnimation;
            this.CurrentPopExitAnimation = currentPopExitAnimation;
            this.ActiveStack = activeStack ?? Array.Empty<StackItem>();
            this.BackStack = backStack ?? Array.Empty<BackStackEntry>();
        }

        public string CurrentDestination { get; }

        public AnchorNavAnimation CurrentPopEnterAnimation { get; }

        public AnchorNavAnimation CurrentPopExitAnimation { get; }

        public IReadOnlyList<StackItem> ActiveStack { get; }

        public IReadOnlyList<BackStackEntry> BackStack { get; }

        public sealed class StackItem
        {
            public StackItem(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, bool isPopup)
            {
                this.Destination = destination;
                this.Options = options?.Clone();
                this.Arguments = arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>();
                this.IsPopup = isPopup;
            }

            public string Destination { get; }

            public AnchorNavOptions Options { get; }

            public AnchorNavArgument[] Arguments { get; }

            public bool IsPopup { get; }
        }

        public sealed class BackStackEntry
        {
            public BackStackEntry(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments, IReadOnlyList<StackItem> snapshot)
            {
                this.Destination = destination;
                this.Options = options?.Clone();
                this.Arguments = arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>();
                this.Snapshot = snapshot ?? Array.Empty<StackItem>();
            }

            public string Destination { get; }

            public AnchorNavOptions Options { get; }

            public AnchorNavArgument[] Arguments { get; }

            public IReadOnlyList<StackItem> Snapshot { get; }
        }
    }
}
