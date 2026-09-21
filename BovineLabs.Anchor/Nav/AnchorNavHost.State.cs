namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class AnchorNavHost
    {
        private readonly Dictionary<int, AnchorNavHostSaveState> _savedStates = new();
        private int _nextStateHandle = 1;

        public AnchorNavHostSaveState SaveState()
        {
            var activeItems = new List<AnchorNavHostSaveState.StackItem>(_activeStack.Count);

            foreach (var entry in _activeStack)
            {
                var savedItem = CreateSavedStackItem(entry);
                if (savedItem != null)
                {
                    activeItems.Add(savedItem);
                }
            }

            var backStackEntries = new List<AnchorNavHostSaveState.BackStackEntry>(_backStack.Count);
            foreach (var entry in _backStack.Reverse())
            {
                var savedEntry = CreateSavedBackStackEntry(entry);
                if (savedEntry != null)
                {
                    backStackEntries.Add(savedEntry);
                }
            }

            return new AnchorNavHostSaveState(
                _currentDestination,
                _currentPopEnterAnimation,
                _currentPopExitAnimation,
                activeItems,
                backStackEntries);
        }

        public void RestoreState(AnchorNavHostSaveState state)
        {
            if (state == null)
            {
                return;
            }

            CancelRunningAnimations();

            while (_activeStack.Count > 0)
            {
                RemoveActiveEntryAt(_activeStack.Count - 1, null);
            }

            _backStack.Clear();

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
                    _backStack.Push(entry);
                }
            }

            var activeSnapshot = CreateSnapshotFromSaved(state.ActiveStack);
            var topOptions = activeSnapshot.Top?.Options;
            ApplySnapshot(activeSnapshot, null, null, topOptions);

            _currentPopEnterAnimation = state.CurrentPopEnterAnimation;
            _currentPopExitAnimation = state.CurrentPopExitAnimation;
            CurrentDestination = state.CurrentDestination;
        }

        public int SaveStateHandle()
        {
            var state = SaveState();
            var handle = _nextStateHandle++;
            _savedStates.Add(handle, state);
            return handle;
        }

        public bool ReleaseStateHandle(int handle, bool restore = true)
        {
            if (!_savedStates.Remove(handle, out var state))
            {
                return false;
            }

            if (restore)
            {
                RestoreState(state);
            }

            return true;
        }

        public IAnchorNavHostReloadState CaptureReloadState()
        {
            return new ReloadState(SaveState(), new Dictionary<int, AnchorNavHostSaveState>(_savedStates), _nextStateHandle);
        }

        public void RestoreReloadState(IAnchorNavHostReloadState state)
        {
            if (state == null)
            {
                return;
            }

            if (state is not ReloadState reloadState)
            {
                throw new ArgumentException(
                    $"Reload state type '{state.GetType().FullName}' was not captured by {nameof(AnchorNavHost)}.",
                    nameof(state));
            }

            RestoreState(reloadState.NavigationState);
            _savedStates.Clear();

            foreach (var pair in reloadState.SavedStates)
            {
                _savedStates.Add(pair.Key, pair.Value);
            }

            _nextStateHandle = reloadState.NextStateHandle;
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

        private sealed class ReloadState : IAnchorNavHostReloadState
        {
            public ReloadState(AnchorNavHostSaveState navigationState, IReadOnlyDictionary<int, AnchorNavHostSaveState> savedStates, int nextStateHandle)
            {
                NavigationState = navigationState;
                SavedStates = savedStates;
                NextStateHandle = nextStateHandle;
            }

            public AnchorNavHostSaveState NavigationState { get; }

            public IReadOnlyDictionary<int, AnchorNavHostSaveState> SavedStates { get; }

            public int NextStateHandle { get; }
        }
    }
}
