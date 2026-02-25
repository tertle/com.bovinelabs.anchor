// <copyright file="AnchorNavHost.State.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class AnchorNavHost
    {
        private readonly Dictionary<int, AnchorNavHostSaveState> savedStates = new();
        private int nextStateHandle = 1;

        /// <summary>
        /// Captures the current visual stack, back stack, and popup configuration so it can be restored later.
        /// </summary>
        /// <returns>A snapshot containing the navigation state that can be supplied to <see cref="RestoreState"/>.</returns>
        public AnchorNavHostSaveState SaveState()
        {
            var activeItems = new List<AnchorNavHostSaveState.StackItem>(this.activeStack.Count);

            foreach (var entry in this.activeStack)
            {
                var savedItem = CreateSavedStackItem(entry);
                if (savedItem != null)
                {
                    activeItems.Add(savedItem);
                }
            }

            var backStackEntries = new List<AnchorNavHostSaveState.BackStackEntry>(this.backStack.Count);
            foreach (var entry in this.backStack.Reverse())
            {
                var savedEntry = CreateSavedBackStackEntry(entry);
                if (savedEntry != null)
                {
                    backStackEntries.Add(savedEntry);
                }
            }

            return new AnchorNavHostSaveState(
                this.currentDestination,
                this.currentPopEnterAnimation,
                this.currentPopExitAnimation,
                activeItems,
                backStackEntries);
        }

        /// <summary>
        /// Restores a previously captured navigation snapshot.
        /// </summary>
        /// <param name="state">Snapshot created via <see cref="SaveState"/>.</param>
        public void RestoreState(AnchorNavHostSaveState state)
        {
            if (state == null)
            {
                return;
            }

            this.CancelRunningAnimations();

            while (this.activeStack.Count > 0)
            {
                this.RemoveActiveEntryAt(this.activeStack.Count - 1, null);
            }

            this.backStack.Clear();

            if (state.BackStack != null)
            {
                foreach (var savedEntry in state.BackStack)
                {
                    var snapshot = CreateSnapshotFromSaved(savedEntry.Snapshot);
                    var entry = new AnchorNavBackStackEntry(
                        savedEntry.Destination,
                        savedEntry.Options?.Clone(),
                        savedEntry.Arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>(),
                        snapshot);
                    this.backStack.Push(entry);
                }
            }

            var activeSnapshot = CreateSnapshotFromSaved(state.ActiveStack);
            var topOptions = activeSnapshot.Top?.Options;
            this.ApplySnapshot(activeSnapshot, null, null, topOptions);

            this.currentPopEnterAnimation = state.CurrentPopEnterAnimation;
            this.currentPopExitAnimation = state.CurrentPopExitAnimation;
            this.CurrentDestination = state.CurrentDestination;
        }

        /// <summary> Captures a navigation state snapshot and returns a handle to restore it later. </summary>
        /// <returns> The handle. </returns>
        public int SaveStateHandle()
        {
            var state = this.SaveState();
            var handle = this.nextStateHandle++;
            this.savedStates.Add(handle, state);
            return handle;
        }

        /// <summary> Releases a navigation state snapshot, optionally restoring it first. </summary>
        /// <param name="handle"> The handle to release. </param>
        /// <param name="restore"> Should the UI state also be restored to the saved snapshot. </param>
        /// <returns> True if the handle existed. </returns>
        public bool ReleaseStateHandle(int handle, bool restore = true)
        {
            if (!this.savedStates.Remove(handle, out var state))
            {
                return false;
            }

            if (restore)
            {
                this.RestoreState(state);
            }

            return true;
        }

        private static AnchorNavHostSaveState.StackItem CreateSavedStackItem(AnchorNavActiveEntry entry)
        {
            return entry == null ? null : new AnchorNavHostSaveState.StackItem(entry.Destination, entry.Options, entry.Arguments, entry.IsPopup);
        }

        private static AnchorNavHostSaveState.StackItem CreateSavedStackItem(AnchorNavStackItem item)
        {
            return item == null ? null : new AnchorNavHostSaveState.StackItem(item.Destination, item.Options, item.Arguments, item.IsPopup);
        }

        private static AnchorNavHostSaveState.BackStackEntry CreateSavedBackStackEntry(AnchorNavBackStackEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            var snapshotItems = entry.Snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var savedSnapshot = new List<AnchorNavHostSaveState.StackItem>(snapshotItems.Count);

            foreach (var item in snapshotItems)
            {
                var savedItem = CreateSavedStackItem(item);
                if (savedItem != null)
                {
                    savedSnapshot.Add(savedItem);
                }
            }

            return new AnchorNavHostSaveState.BackStackEntry(entry.Destination, entry.Options, entry.Arguments, savedSnapshot);
        }

        private static AnchorNavStackSnapshot CreateSnapshotFromSaved(IReadOnlyList<AnchorNavHostSaveState.StackItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            var stackItems = new List<AnchorNavStackItem>(items.Count);
            foreach (var item in items)
            {
                var stackItem = CreateStackItem(item);
                if (stackItem != null)
                {
                    stackItems.Add(stackItem);
                }
            }

            if (stackItems.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            return new AnchorNavStackSnapshot(stackItems);
        }

        private static AnchorNavStackItem CreateStackItem(AnchorNavHostSaveState.StackItem item)
        {
            if (item == null)
            {
                return null;
            }

            var options = item.Options?.Clone();
            var arguments = item.Arguments?.ToArray() ?? Array.Empty<AnchorNavArgument>();
            return new AnchorNavStackItem(item.Destination, options, arguments, item.IsPopup);
        }
    }
}
