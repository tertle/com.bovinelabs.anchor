namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Core;
    using UnityEngine.UIElements;

    public partial class AnchorNavHost
    {
        /// <summary>
        /// Leaves the current destination unchanged.
        /// </summary>
        public void ClearBackStack()
        {
            _backStack.Clear();
        }

        public void ClearNavigation(int exitAnimation = 0)
        {
            var animation = ResolveAnimation(exitAnimation);
            _backStack.Clear();
            ApplySnapshot(AnchorNavStackSnapshot.Empty, animation, null, new AnchorNavOptions());
        }

        public bool Navigate(string actionOrDestination, AnchorNavArgument argument)
        {
            return Navigate(actionOrDestination, new[] { argument });
        }

        public bool Navigate(string actionOrDestination, AnchorNavArgument[] arguments = null)
        {
            if (!TryResolveActionOrDestination(actionOrDestination, arguments, out var destination, out var options, out var mergedArguments))
            {
                return false;
            }

            return Navigate(destination, options, mergedArguments);
        }

        public bool Navigate(string destination, AnchorNavOptions options, AnchorNavArgument argument)
        {
            return Navigate(destination, options, new[] { argument });
        }

        public bool Navigate(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments = null)
        {
            options ??= new AnchorNavOptions();

            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<AnchorNavArgument>();

            if (options.PopupStrategy != AnchorPopupStrategy.None)
            {
                return NavigatePopupInternal(destination, options, arguments);
            }

            if (_activeStack.Count > 0)
            {
                var currentSnapshot = CaptureCurrentSnapshot();
                var canPush = CurrentBackStackEntry == null ||
                    CurrentBackStackEntry.Destination != CurrentDestination;

                if (canPush)
                {
                    PushSnapshot(currentSnapshot);
                }
            }

            switch (options.StackStrategy)
            {
                case AnchorStackStrategy.PopAll:
                {
                    _backStack.Clear();
                    break;
                }

                case AnchorStackStrategy.PopToRoot when _backStack.Count > 0:
                {
                    var rootDestination = _backStack.ElementAt(_backStack.Count - 1).Destination;
                    PopUpTo(rootDestination);
                    break;
                }

                case AnchorStackStrategy.PopToSpecificDestination when !string.IsNullOrWhiteSpace(options.PopupToDestination):
                {
                    PopUpTo(options.PopupToDestination);
                    break;
                }
            }

            if (destination == CurrentBackStackEntry?.Destination)
            {
                _backStack.Pop();
            }

            NavigateInternal(destination, options, arguments);
            return true;
        }

        public bool Toggle(string actionOrDestination, AnchorNavArgument argument)
        {
            return Toggle(actionOrDestination, new[] { argument });
        }

        public bool Toggle(string actionOrDestination, AnchorNavArgument[] arguments = null)
        {
            if (!TryResolveActionOrDestination(actionOrDestination, arguments, out var destination, out var options, out var mergedArguments))
            {
                return false;
            }

            if (TryDismissActivePopupBranch(destination))
            {
                return true;
            }

            return Navigate(destination, options, mergedArguments);
        }

        public bool PopBackStack()
        {
            return PopBackStack(clearPopups: false);
        }

        public bool PopBackStackToPanel()
        {
            return PopBackStack(clearPopups: true);
        }

        public bool CloseAllPopups(int exitAnimation = 0)
        {
            var startIndex = FindFirstActivePopupIndex();
            if (startIndex < 0)
            {
                return false;
            }

            var animation = ResolveAnimation(exitAnimation);
            RemoveActiveEntriesFrom(startIndex, animation);
            UpdateCurrentFromActiveStack();
            return true;
        }

        public bool ClosePopup(string destination, int exitAnimation = 0)
        {
            if (string.IsNullOrWhiteSpace(destination) || _activeStack.Count == 0)
            {
                return false;
            }

            var index = FindActivePopupIndex(destination);
            if (index < 0)
            {
                return false;
            }

            var animation = ResolveAnimation(exitAnimation);
            RemoveActiveEntryAt(index, animation);
            UpdateCurrentFromActiveStack();
            return true;
        }

        private bool PopBackStack(bool clearPopups)
        {
            if (_backStack.Count == 0)
            {
                if (clearPopups)
                {
                    return CloseAllPopups();
                }

                return false;
            }

            var entry = _backStack.Pop();
            var snapshot = clearPopups ? entry.Snapshot.WithoutPopups() : entry.Snapshot;

            if (!ReferenceEquals(snapshot, entry.Snapshot))
            {
                entry = new AnchorNavBackStackEntry(entry.Destination, entry.Options, entry.Arguments, snapshot);
            }

            HandlePopBack(entry);
            return true;
        }

        private void NavigateInternal(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            options ??= new AnchorNavOptions();
            arguments ??= Array.Empty<AnchorNavArgument>();

            if (options.PopupStrategy != AnchorPopupStrategy.None)
            {
                NavigatePopupInternal(destination, options, arguments);
                return;
            }

            var snapshot = new AnchorNavStackSnapshot(new[]
            {
                new AnchorNavStackItem(destination, options, arguments, false),
            });

            HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, snapshot));
        }

        private bool NavigatePopupInternal(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            if (string.IsNullOrEmpty(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<AnchorNavArgument>();

            switch (options.PopupStrategy)
            {
                case AnchorPopupStrategy.PopupOnCurrent:
                    return NavigatePopupOnCurrent(destination, options, arguments);
                case AnchorPopupStrategy.EnsureBaseAndPopup:
                    return NavigatePopupEnsureBase(destination, options, arguments);
                case AnchorPopupStrategy.None:
                default:
                    NavigateInternal(destination, options, arguments);
                    return true;
            }
        }

        private bool NavigatePopupEnsureBase(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            var baseDestination = options.PopupBaseDestination;
            if (string.IsNullOrWhiteSpace(baseDestination))
            {
                BLGlobalLogger.LogErrorString($"Popup strategy {AnchorPopupStrategy.EnsureBaseAndPopup} requires a base destination.");
                return false;
            }

            var baseAligned = TryGetCurrentBase(out var currentBase) && currentBase.Destination == baseDestination;

            if (!baseAligned)
            {
                var baseOptions = options.Clone();
                baseOptions.PopupStrategy = AnchorPopupStrategy.None;
                baseOptions.PopupBaseDestination = null;
                baseOptions.PopupBaseArguments.Clear();

                var baseArgsList = options.PopupBaseArguments;
                var baseArgs = baseArgsList is { Count: > 0 } ? baseArgsList.ToArray() : Array.Empty<AnchorNavArgument>();

                if (!Navigate(baseDestination, baseOptions, baseArgs))
                {
                    return false;
                }
            }

            return NavigatePopupOnCurrent(destination, options, arguments);
        }

        private bool NavigatePopupOnCurrent(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            if (_activeStack.Count == 0)
            {
                BLGlobalLogger.LogWarningString($"Popup navigation to '{destination}' requested without an active base destination. Falling back to normal navigation.");

                var fallbackOptions = options.Clone();
                fallbackOptions.PopupStrategy = AnchorPopupStrategy.None;
                fallbackOptions.PopupBaseDestination = null;
                fallbackOptions.PopupBaseArguments.Clear();
                fallbackOptions.PopupExistingStrategy = AnchorPopupExistingStrategy.None;
                NavigateInternal(destination, fallbackOptions, arguments);
                return true;
            }

            var currentSnapshot = CaptureCurrentSnapshot();
            var topEntry = _activeStack[^1];
            var handling = options.PopupExistingStrategy;
            var hasExistingPopups = currentSnapshot.HasPopups;

            if (handling == AnchorPopupExistingStrategy.None || (!hasExistingPopups && handling != AnchorPopupExistingStrategy.PushNew))
            {
                if (topEntry.Destination == destination && topEntry.IsPopup)
                {
                    var updatedItems = currentSnapshot.Items.ToList();
                    updatedItems[^1] = new AnchorNavStackItem(destination, options, arguments, true);
                    var updatedSnapshot = new AnchorNavStackSnapshot(updatedItems);
                    HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, updatedSnapshot));
                    return true;
                }

                PushSnapshot(currentSnapshot);

                var stackedItems = currentSnapshot.Items.ToList();
                stackedItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
                var stackedSnapshot = new AnchorNavStackSnapshot(stackedItems);

                HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, stackedSnapshot));
                return true;
            }

            var pushedExistingSnapshot = false;
            if (handling == AnchorPopupExistingStrategy.PushNew)
            {
                PushSnapshot(currentSnapshot);
                pushedExistingSnapshot = true;
            }

            var baseItems = new List<AnchorNavStackItem>(currentSnapshot.Items.Count);
            foreach (var item in currentSnapshot.Items)
            {
                if (!item.IsPopup)
                {
                    baseItems.Add(item);
                }
            }

            if (baseItems.Count == 0)
            {
                if (!pushedExistingSnapshot)
                {
                    PushSnapshot(currentSnapshot);
                }

                var fallbackItems = currentSnapshot.Items.ToList();
                fallbackItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
                var fallbackSnapshot = new AnchorNavStackSnapshot(fallbackItems);
                HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, fallbackSnapshot));
                return true;
            }

            baseItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
            var targetSnapshot = new AnchorNavStackSnapshot(baseItems);

            HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, targetSnapshot));
            return true;
        }

        private void PopUpTo(string destination)
        {
            while (_backStack.TryPeek(out var entry))
            {
                if (entry.Destination == destination)
                {
                    break;
                }

                _backStack.Pop();
            }
        }

        private void HandleNavigate(AnchorNavBackStackEntry entry)
        {
            _currentPopEnterAnimation = entry.Options.Animations.PopEnterAnim;
            _currentPopExitAnimation = entry.Options.Animations.PopExitAnim;

            ApplySnapshot(
                entry.Snapshot,
                entry.Options.Animations.ExitAnim,
                entry.Options.Animations.EnterAnim,
                entry.Options);

            CurrentDestination = entry.Destination;
        }

        private void HandlePopBack(AnchorNavBackStackEntry entry)
        {
            var exitAnim = _currentPopExitAnimation;
            var enterAnim = _currentPopEnterAnimation;

            ApplySnapshot(
                entry.Snapshot,
                exitAnim,
                enterAnim,
                entry.Options);

            CurrentDestination = entry.Destination;
        }

        private AnchorNavAnimation ResolveAnimation(int id)
        {
            if (TryGetAnimation(id, out var animation))
            {
                return animation;
            }

            BLGlobalLogger.LogWarningString($"AnchorNavHost could not find animation '{id}'.");
            return null;
        }

        private void ApplySnapshot(AnchorNavStackSnapshot snapshot, AnchorNavAnimation exitAnim, AnchorNavAnimation enterAnim, AnchorNavOptions optionsForTop)
        {
            optionsForTop ??= new AnchorNavOptions();

            var targetItems = snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var sharedPrefix = GetSharedPrefix(targetItems);

            for (var i = 0; i < sharedPrefix; i++)
            {
                var entry = _activeStack[i];
                var targetItem = targetItems[i];
                var argumentsChanged = !ArgumentsEqual(entry.Arguments, targetItem.Arguments);
                entry.Update(targetItem);

                if (argumentsChanged)
                {
                    OnEntered(entry);
                }
            }

            CancelRunningAnimations();

            for (var i = _activeStack.Count - 1; i >= sharedPrefix; i--)
            {
                RemoveActiveEntryAt(i, exitAnim);
            }

            for (var i = sharedPrefix; i < targetItems.Count; i++)
            {
                var item = targetItems[i];
                var animation = i == targetItems.Count - 1 ? enterAnim : null;
                AddActiveEntry(i, item, animation);
            }

            var top = targetItems.Count > 0 ? targetItems[^1] : null;
            CurrentDestination = top?.Destination;

            _currentPopEnterAnimation = optionsForTop.Animations.PopEnterAnim;
            _currentPopExitAnimation = optionsForTop.Animations.PopExitAnim;
        }

        private AnchorNavStackSnapshot CaptureCurrentSnapshot()
        {
            if (_activeStack.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            var items = new List<AnchorNavStackItem>(_activeStack.Count);
            foreach (var entry in _activeStack)
            {
                items.Add(new AnchorNavStackItem(entry.Destination, entry.Options, entry.Arguments, entry.IsPopup));
            }

            return new AnchorNavStackSnapshot(items);
        }

        private void PushSnapshot(AnchorNavStackSnapshot snapshot)
        {
            if (snapshot?.Top == null)
            {
                return;
            }

            var top = snapshot.Top;
            var entry = new AnchorNavBackStackEntry(top.Destination, top.Options, top.Arguments, snapshot);
            _backStack.Push(entry);
        }

        private void TrimBackStackToActive()
        {
            while (_backStack.TryPeek(out var entry))
            {
                if (!MatchesActiveStack(entry.Snapshot))
                {
                    break;
                }

                _backStack.Pop();
            }
        }

        private void AddActiveEntry(int index, AnchorNavStackItem item, AnchorNavAnimation enterAnim)
        {
            var element = CreateItem(item.Destination);

            if (index >= _container.childCount)
            {
                _container.Add(element);
            }
            else
            {
                _container.Insert(index, element);
            }

            var entry = new AnchorNavActiveEntry(item.Destination, item.Arguments, item.IsPopup, item.Options, element);
            _activeStack.Insert(index, entry);

            TryPlayAnimation(element, enterAnim, null);

            OnEntered(entry);
            EnteredDestination?.Invoke(this, element, entry.Arguments);
        }

        private void RemoveActiveEntryAt(int index, AnchorNavAnimation exitAnim)
        {
            var entry = _activeStack[index];
            _activeStack.RemoveAt(index);

            OnExit(entry);
            ExitedDestination?.Invoke(this, entry.Element, entry.Arguments);

            CompleteAnimationsFor(entry.Element);

            void OnCompleted()
            {
                if (entry.Element.parent != null)
                {
                    entry.Element.RemoveFromHierarchy();
                }
            }

            if (!TryPlayAnimation(entry.Element, exitAnim, OnCompleted))
            {
                OnCompleted();
            }
        }

        private static void OnEntered(AnchorNavActiveEntry entry)
        {
            var handled = false;

            foreach (var ve in entry.Element.Query<VisualElement>().Build())
            {
                if (ve.dataSource is IAnchorNavigationScreen screen)
                {
                    screen.OnEnter(entry.Arguments);
                    handled = true;
                }
            }

            if (!handled && entry.Arguments is { Length: > 0 })
            {
                BLGlobalLogger.LogWarningString($"AnchorNavHost received navigation arguments for '{entry.Destination}' but no screen handled OnEnter.");
            }
        }

        private static void OnExit(AnchorNavActiveEntry entry)
        {
            var handled = false;

            foreach (var ve in entry.Element.Query<VisualElement>().Build())
            {
                if (ve.dataSource is IAnchorNavigationScreen screen)
                {
                    screen.OnExit(entry.Arguments);
                    handled = true;
                }
            }

            if (!handled && entry.Arguments is { Length: > 0 })
            {
                BLGlobalLogger.LogWarningString($"AnchorNavHost received navigation arguments for '{entry.Destination}' but no screen handled OnExit.");
            }
        }

        private bool TryPlayAnimation(VisualElement element, AnchorNavAnimation animation, Action onCompleted)
        {
            var description = GetAnimationDescription(animation);
            if (description is { DurationMs: <= 0, Callback: null })
            {
                onCompleted?.Invoke();
                return false;
            }

            var handleInfo = new AnchorNavAnimationHandle(element, description, onCompleted);
            var handle = element.experimental.animation
                .Start(0, 1, description.DurationMs, description.Callback)
                .Ease(description.Easing)
                .OnCompleted(() =>
                {
                    if (!handleInfo.TryFinalizeFromAnimation())
                    {
                        return;
                    }

                    onCompleted?.Invoke();
                    _runningAnimations.Remove(handleInfo);
                })
                .KeepAlive();

            handleInfo.Handle = handle;
            _runningAnimations.Add(handleInfo);
            return true;
        }

        private static AnimationDescription GetAnimationDescription(AnchorNavAnimation animation)
        {
            return animation != null ? animation.GetDescription() : AnimationDescription.None;
        }

        private void CancelRunningAnimations()
        {
            if (_runningAnimations.Count == 0)
            {
                return;
            }

            foreach (var animation in _runningAnimations)
            {
                animation.CompleteImmediately();
            }

            _runningAnimations.Clear();
        }

        private void CompleteAnimationsFor(VisualElement element)
        {
            for (var i = _runningAnimations.Count - 1; i >= 0; i--)
            {
                var handle = _runningAnimations[i];
                if (handle.Element != element)
                {
                    continue;
                }

                handle.CompleteImmediately();
                _runningAnimations.RemoveAt(i);
            }
        }

        private bool TryResolveActionOrDestination(
            string actionOrDestination, AnchorNavArgument[] arguments, out string destination, out AnchorNavOptions options,
            out AnchorNavArgument[] mergedArguments)
        {
            destination = null;
            options = null;
            mergedArguments = arguments ?? Array.Empty<AnchorNavArgument>();

            if (string.IsNullOrWhiteSpace(actionOrDestination))
            {
                return false;
            }

            if (!_actions.TryGetValue(actionOrDestination, out var action))
            {
                destination = actionOrDestination;
                return true;
            }

            ActionTriggered?.Invoke(this, action);
            destination = action.Destination;
            options = action.Options?.Clone() ?? new AnchorNavOptions();
            mergedArguments = action.MergeArguments(arguments);
            return true;
        }

        private bool TryDismissActivePopupBranch(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            var index = FindActivePopupIndex(destination);
            if (index < 0)
            {
                return false;
            }

            RemoveActiveEntriesFrom(index, null);
            UpdateCurrentFromActiveStack();
            return true;
        }

        private int FindFirstActivePopupIndex()
        {
            for (var i = _activeStack.Count - 1; i >= 0; i--)
            {
                if (!_activeStack[i].IsPopup)
                {
                    return i == _activeStack.Count - 1 ? -1 : i + 1;
                }
            }

            return _activeStack.Count > 0 ? 0 : -1;
        }

        private int FindActivePopupIndex(string destination)
        {
            for (var i = _activeStack.Count - 1; i >= 0; i--)
            {
                var entry = _activeStack[i];
                if (!entry.IsPopup)
                {
                    break;
                }

                if (entry.Destination == destination)
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveActiveEntriesFrom(int index, AnchorNavAnimation exitAnim)
        {
            for (var i = _activeStack.Count - 1; i >= index; i--)
            {
                RemoveActiveEntryAt(i, exitAnim);
            }
        }

        private void UpdateCurrentFromActiveStack()
        {
            TrimBackStackToActive();

            var top = _activeStack.Count > 0 ? _activeStack[^1] : null;
            CurrentDestination = top?.Destination;
            _currentPopEnterAnimation = top?.Options.Animations.PopEnterAnim;
            _currentPopExitAnimation = top?.Options.Animations.PopExitAnim;
        }

        private bool TryGetCurrentBase(out AnchorNavActiveEntry entry)
        {
            for (var i = _activeStack.Count - 1; i >= 0; i--)
            {
                var candidate = _activeStack[i];
                if (!candidate.IsPopup)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        private int GetSharedPrefix(IReadOnlyList<AnchorNavStackItem> targetItems)
        {
            var count = Math.Min(_activeStack.Count, targetItems.Count);
            var prefix = 0;

            while (prefix < count && AreEquivalent(_activeStack[prefix], targetItems[prefix]))
            {
                prefix++;
            }

            return prefix;
        }

        private bool AreEquivalent(AnchorNavActiveEntry existing, AnchorNavStackItem target)
        {
            if (existing == null || target == null)
            {
                return false;
            }

            return existing.Destination == target.Destination && existing.IsPopup == target.IsPopup;
        }

        private static bool ArgumentsEqual(AnchorNavArgument[] left, AnchorNavArgument[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (!EqualityComparer<AnchorNavArgument>.Default.Equals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MatchesActiveStack(AnchorNavStackSnapshot snapshot)
        {
            var items = snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();

            if (items.Count != _activeStack.Count)
            {
                return false;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var target = items[i];
                var existing = _activeStack[i];

                if (existing.Destination != target.Destination || existing.IsPopup != target.IsPopup)
                {
                    return false;
                }
            }

            return true;
        }

        private VisualElement CreateItem(string destination)
        {
            var element = AnchorApp.Current.Services.GetService<IUXMLService>().Instantiate(destination);
            element.StretchToParentSize();
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }
}
