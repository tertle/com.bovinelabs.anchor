// <copyright file="AnchorNavHostSaveState.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.AppUI.Navigation;

    /// <summary>
    /// Serializable snapshot of an <see cref="AnchorNavHost"/> state.
    /// </summary>
    public sealed class AnchorNavHostSaveState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavHostSaveState"/> class.
        /// </summary>
        /// <param name="currentDestination">The current top-most destination.</param>
        /// <param name="currentPopEnterAnimation">Animation that should play when re-entering after a pop.</param>
        /// <param name="currentPopExitAnimation">Animation that should play when exiting during a pop.</param>
        /// <param name="activeStack">Snapshot of the active visual stack.</param>
        /// <param name="backStack">Snapshot of the back stack entries.</param>
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

        /// <summary> Gets the current top-most destination that was active when the snapshot was taken. </summary>
        public string CurrentDestination { get; }

        /// <summary> Gets the animation that should play when an entry re-enters the stack after a pop. </summary>
        public AnchorNavAnimation CurrentPopEnterAnimation { get; }

        /// <summary> Gets the animation that should play when popping the current destination. </summary>
        public AnchorNavAnimation CurrentPopExitAnimation { get; }

        /// <summary> Gets the ordered list representing the currently active visual stack. </summary>
        public IReadOnlyList<StackItem> ActiveStack { get; }

        /// <summary> Gets the ordered list representing the stored back stack entries. </summary>
        public IReadOnlyList<BackStackEntry> BackStack { get; }

        /// <summary>
        /// Serializable representation of a single active stack item.
        /// </summary>
        public sealed class StackItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StackItem"/> class.
            /// </summary>
            /// <param name="destination">Destination identifier.</param>
            /// <param name="options">Navigation options associated with the destination.</param>
            /// <param name="arguments">Arguments supplied when the destination was navigated to.</param>
            /// <param name="isPopup">Whether the destination was displayed as a popup.</param>
            public StackItem(string destination, AnchorNavOptions options, Argument[] arguments, bool isPopup)
            {
                this.Destination = destination;
                this.Options = options?.Clone();
                this.Arguments = arguments?.ToArray() ?? Array.Empty<Argument>();
                this.IsPopup = isPopup;
            }

            /// <summary> Gets the destination identifier. </summary>
            public string Destination { get; }

            /// <summary> Gets the options associated with the destination. </summary>
            public AnchorNavOptions Options { get; }

            /// <summary> Gets the arguments supplied when the destination was navigated to. </summary>
            public Argument[] Arguments { get; }

            /// <summary> Gets a value indicating whether the destination was displayed as a popup. </summary>
            public bool IsPopup { get; }
        }

        /// <summary>
        /// Serializable representation of a back stack entry.
        /// </summary>
        public sealed class BackStackEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BackStackEntry"/> class.
            /// </summary>
            /// <param name="destination">Destination identifier.</param>
            /// <param name="options">Navigation options associated with the entry.</param>
            /// <param name="arguments">Arguments supplied when the entry was created.</param>
            /// <param name="snapshot">Saved snapshot of the visual stack for the entry.</param>
            public BackStackEntry(string destination, AnchorNavOptions options, Argument[] arguments, IReadOnlyList<StackItem> snapshot)
            {
                this.Destination = destination;
                this.Options = options?.Clone();
                this.Arguments = arguments?.ToArray() ?? Array.Empty<Argument>();
                this.Snapshot = snapshot ?? Array.Empty<StackItem>();
            }

            /// <summary> Gets the destination identifier. </summary>
            public string Destination { get; }

            /// <summary> Gets the navigation options associated with the entry. </summary>
            public AnchorNavOptions Options { get; }

            /// <summary> Gets the arguments supplied when the entry was added to the back stack. </summary>
            public Argument[] Arguments { get; }

            /// <summary> Gets the snapshot that should be applied when the entry becomes active. </summary>
            public IReadOnlyList<StackItem> Snapshot { get; }
        }
    }
}
