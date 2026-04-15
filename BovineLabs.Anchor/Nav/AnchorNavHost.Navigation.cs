// <copyright file="AnchorNavHost.Navigation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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
        /// <summary> Clear the back stack entirely. </summary>
        /// <remarks> The current destination remains unchanged. </remarks>
        public void ClearBackStack()
        {
            this.backStack.Clear();
        }

        /// <summary> Clear the active stack and back stack so no destination remains active. </summary>
        /// <param name="exitAnimation"> Optional animation to use when removing the last active entry. </param>
        public void ClearNavigation(int exitAnimation = 0)
        {
            var animation = this.ResolveAnimation(exitAnimation);
            this.backStack.Clear();
            this.ApplySnapshot(AnchorNavStackSnapshot.Empty, animation, null, new AnchorNavOptions());
        }

        /// <summary> Navigate to the destination with the given name. </summary>
        /// <param name="actionOrDestination"> The name of the action. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(string actionOrDestination, params AnchorNavArgument[] arguments)
        {
            if (!this.TryResolveActionOrDestination(actionOrDestination, arguments, out var destination, out var options, out var mergedArguments))
            {
                return false;
            }

            return this.Navigate(destination, options, mergedArguments);
        }

        /// <summary> Navigate to the destination with the given name. </summary>
        /// <param name="destination"> The destination. </param>
        /// <param name="options"> The options to use for the navigation. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(string destination, AnchorNavOptions options, params AnchorNavArgument[] arguments)
        {
            options ??= new AnchorNavOptions();

            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<AnchorNavArgument>();

            if (options.PopupStrategy != AnchorPopupStrategy.None)
            {
                return this.NavigatePopupInternal(destination, options, arguments);
            }

            if (this.activeStack.Count > 0)
            {
                var currentSnapshot = this.CaptureCurrentSnapshot();
                var canPush = this.CurrentBackStackEntry == null ||
                    this.CurrentBackStackEntry.Destination != this.CurrentDestination;

                if (canPush)
                {
                    this.PushSnapshot(currentSnapshot);
                }
            }

            switch (options.StackStrategy)
            {
                case AnchorStackStrategy.PopAll:
                {
                    this.backStack.Clear();
                    break;
                }

                case AnchorStackStrategy.PopToRoot when this.backStack.Count > 0:
                {
                    var rootDestination = this.backStack.ElementAt(this.backStack.Count - 1).Destination;
                    this.PopUpTo(rootDestination);
                    break;
                }

                case AnchorStackStrategy.PopToSpecificDestination when !string.IsNullOrWhiteSpace(options.PopupToDestination):
                {
                    this.PopUpTo(options.PopupToDestination);
                    break;
                }
            }

            if (destination == this.CurrentBackStackEntry?.Destination)
            {
                this.backStack.Pop();
            }

            this.NavigateInternal(destination, options, arguments);
            return true;
        }

        /// <summary>
        /// Toggle a popup destination or action by dismissing the matching popup branch when active, otherwise navigating.
        /// </summary>
        /// <param name="actionOrDestination">The action or destination to toggle.</param>
        /// <param name="arguments">The arguments to pass when navigating.</param>
        /// <returns>True if the toggle was successful.</returns>
        public bool Toggle(string actionOrDestination, params AnchorNavArgument[] arguments)
        {
            if (!this.TryResolveActionOrDestination(actionOrDestination, arguments, out var destination, out var options, out var mergedArguments))
            {
                return false;
            }

            if (this.TryDismissActivePopupBranch(destination))
            {
                return true;
            }

            return this.Navigate(destination, options, mergedArguments);
        }

        /// <summary> Pop the current destination from the back stack and navigate to the previous destination. </summary>
        /// <returns> True if the back stack was popped, false otherwise. </returns>
        public bool PopBackStack()
        {
            return this.PopBackStack(clearPopups: false);
        }

        /// <summary> Pop the current destination and clear any popup overlays that were captured with it. </summary>
        /// <returns> True if the back stack or popup stack was updated, false otherwise. </returns>
        public bool PopBackStackToPanel()
        {
            return this.PopBackStack(clearPopups: true);
        }

        /// <summary> Close all currently-displayed popup overlays. </summary>
        /// <param name="exitAnimation"> Optional animation name to play when dismissing each popup. </param>
        /// <returns> True if at least one popup was closed. </returns>
        public bool CloseAllPopups(int exitAnimation = 0)
        {
            var startIndex = this.FindFirstActivePopupIndex();
            if (startIndex < 0)
            {
                return false;
            }

            var animation = this.ResolveAnimation(exitAnimation);
            this.RemoveActiveEntriesFrom(startIndex, animation);
            this.UpdateCurrentFromActiveStack();
            return true;
        }

        /// <summary> Close a popup in the active stack that matches the provided destination. </summary>
        /// <param name="destination"> The destination key of the popup to close. </param>
        /// <param name="exitAnimation"> Optional animation name to play when dismissing the popup. </param>
        /// <returns> True if the popup was closed; otherwise, false. </returns>
        public bool ClosePopup(string destination, int exitAnimation = 0)
        {
            if (string.IsNullOrWhiteSpace(destination) || this.activeStack.Count == 0)
            {
                return false;
            }

            var index = this.FindActivePopupIndex(destination);
            if (index < 0)
            {
                return false;
            }

            var animation = this.ResolveAnimation(exitAnimation);
            this.RemoveActiveEntryAt(index, animation);
            this.UpdateCurrentFromActiveStack();
            return true;
        }

        private bool PopBackStack(bool clearPopups)
        {
            if (this.backStack.Count == 0)
            {
                if (clearPopups)
                {
                    return this.CloseAllPopups();
                }

                return false;
            }

            var entry = this.backStack.Pop();
            var snapshot = clearPopups ? entry.Snapshot.WithoutPopups() : entry.Snapshot;

            if (!ReferenceEquals(snapshot, entry.Snapshot))
            {
                entry = new AnchorNavBackStackEntry(entry.Destination, entry.Options, entry.Arguments, snapshot);
            }

            this.HandlePopBack(entry);
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
                this.NavigatePopupInternal(destination, options, arguments);
                return;
            }

            var snapshot = new AnchorNavStackSnapshot(new[]
            {
                new AnchorNavStackItem(destination, options, arguments, false),
            });

            this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, snapshot));
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
                    return this.NavigatePopupOnCurrent(destination, options, arguments);
                case AnchorPopupStrategy.EnsureBaseAndPopup:
                    return this.NavigatePopupEnsureBase(destination, options, arguments);
                case AnchorPopupStrategy.None:
                default:
                    this.NavigateInternal(destination, options, arguments);
                    return true;
            }
        }

        private bool NavigatePopupEnsureBase(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            var baseDestination = options.PopupBaseDestination;
            if (string.IsNullOrWhiteSpace(baseDestination))
            {
                BLGlobalLogger.LogError($"Popup strategy {AnchorPopupStrategy.EnsureBaseAndPopup} requires a base destination.");
                return false;
            }

            var baseAligned = this.TryGetCurrentBase(out var currentBase) && currentBase.Destination == baseDestination;

            if (!baseAligned)
            {
                var baseOptions = options.Clone();
                baseOptions.PopupStrategy = AnchorPopupStrategy.None;
                baseOptions.PopupBaseDestination = null;
                baseOptions.PopupBaseArguments.Clear();

                var baseArgsList = options.PopupBaseArguments;
                var baseArgs = baseArgsList is { Count: > 0 } ? baseArgsList.ToArray() : Array.Empty<AnchorNavArgument>();

                if (!this.Navigate(baseDestination, baseOptions, baseArgs))
                {
                    return false;
                }
            }

            return this.NavigatePopupOnCurrent(destination, options, arguments);
        }

        private bool NavigatePopupOnCurrent(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments)
        {
            if (this.activeStack.Count == 0)
            {
                BLGlobalLogger.LogWarningString($"Popup navigation to '{destination}' requested without an active base destination. Falling back to normal navigation.");

                var fallbackOptions = options.Clone();
                fallbackOptions.PopupStrategy = AnchorPopupStrategy.None;
                fallbackOptions.PopupBaseDestination = null;
                fallbackOptions.PopupBaseArguments.Clear();
                fallbackOptions.PopupExistingStrategy = AnchorPopupExistingStrategy.None;
                this.NavigateInternal(destination, fallbackOptions, arguments);
                return true;
            }

            var currentSnapshot = this.CaptureCurrentSnapshot();
            var topEntry = this.activeStack[^1];
            var handling = options.PopupExistingStrategy;
            var hasExistingPopups = currentSnapshot.HasPopups;

            if (handling == AnchorPopupExistingStrategy.None || (!hasExistingPopups && handling != AnchorPopupExistingStrategy.PushNew))
            {
                if (topEntry.Destination == destination && topEntry.IsPopup)
                {
                    var updatedItems = currentSnapshot.Items.ToList();
                    updatedItems[^1] = new AnchorNavStackItem(destination, options, arguments, true);
                    var updatedSnapshot = new AnchorNavStackSnapshot(updatedItems);
                    this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, updatedSnapshot));
                    return true;
                }

                this.PushSnapshot(currentSnapshot);

                var stackedItems = currentSnapshot.Items.ToList();
                stackedItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
                var stackedSnapshot = new AnchorNavStackSnapshot(stackedItems);

                this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, stackedSnapshot));
                return true;
            }

            var pushedExistingSnapshot = false;
            if (handling == AnchorPopupExistingStrategy.PushNew)
            {
                this.PushSnapshot(currentSnapshot);
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
                    this.PushSnapshot(currentSnapshot);
                }

                var fallbackItems = currentSnapshot.Items.ToList();
                fallbackItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
                var fallbackSnapshot = new AnchorNavStackSnapshot(fallbackItems);
                this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, fallbackSnapshot));
                return true;
            }

            baseItems.Add(new AnchorNavStackItem(destination, options, arguments, true));
            var targetSnapshot = new AnchorNavStackSnapshot(baseItems);

            this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, targetSnapshot));
            return true;
        }

        private void PopUpTo(string destination)
        {
            while (this.backStack.TryPeek(out var entry))
            {
                if (entry.Destination == destination)
                {
                    break;
                }

                this.backStack.Pop();
            }
        }

        private void HandleNavigate(AnchorNavBackStackEntry entry)
        {
            this.currentPopEnterAnimation = entry.Options.Animations.PopEnterAnim;
            this.currentPopExitAnimation = entry.Options.Animations.PopExitAnim;

            this.ApplySnapshot(
                entry.Snapshot,
                entry.Options.Animations.ExitAnim,
                entry.Options.Animations.EnterAnim,
                entry.Options);

            this.CurrentDestination = entry.Destination;
        }

        private void HandlePopBack(AnchorNavBackStackEntry entry)
        {
            var exitAnim = this.currentPopExitAnimation;
            var enterAnim = this.currentPopEnterAnimation;

            this.ApplySnapshot(
                entry.Snapshot,
                exitAnim,
                enterAnim,
                entry.Options);

            this.CurrentDestination = entry.Destination;
        }

        private AnchorNavAnimation ResolveAnimation(int id)
        {
            if (this.TryGetAnimation(id, out var animation))
            {
                return animation;
            }

            BLGlobalLogger.LogWarningString($"AnchorNavHost could not find animation '{id}'.");
            return null;
        }

        private void ApplySnapshot(
            AnchorNavStackSnapshot snapshot,
            AnchorNavAnimation exitAnim,
            AnchorNavAnimation enterAnim,
            AnchorNavOptions optionsForTop)
        {
            optionsForTop ??= new AnchorNavOptions();

            var targetItems = snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var sharedPrefix = this.GetSharedPrefix(targetItems);

            for (var i = 0; i < sharedPrefix; i++)
            {
                var entry = this.activeStack[i];
                var targetItem = targetItems[i];
                var argumentsChanged = !ArgumentsEqual(entry.Arguments, targetItem.Arguments);
                entry.Update(targetItem);

                if (argumentsChanged)
                {
                    OnEntered(entry);
                }
            }

            this.CancelRunningAnimations();

            for (var i = this.activeStack.Count - 1; i >= sharedPrefix; i--)
            {
                this.RemoveActiveEntryAt(i, exitAnim);
            }

            for (var i = sharedPrefix; i < targetItems.Count; i++)
            {
                var item = targetItems[i];
                var animation = i == targetItems.Count - 1 ? enterAnim : null;
                this.AddActiveEntry(i, item, animation);
            }

            var top = targetItems.Count > 0 ? targetItems[^1] : null;
            this.CurrentDestination = top?.Destination;

            this.currentPopEnterAnimation = optionsForTop.Animations.PopEnterAnim;
            this.currentPopExitAnimation = optionsForTop.Animations.PopExitAnim;
        }

        private AnchorNavStackSnapshot CaptureCurrentSnapshot()
        {
            if (this.activeStack.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            var items = new List<AnchorNavStackItem>(this.activeStack.Count);
            foreach (var entry in this.activeStack)
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
            this.backStack.Push(entry);
        }

        private void TrimBackStackToActive()
        {
            while (this.backStack.TryPeek(out var entry))
            {
                if (!this.MatchesActiveStack(entry.Snapshot))
                {
                    break;
                }

                this.backStack.Pop();
            }
        }

        private void AddActiveEntry(int index, AnchorNavStackItem item, AnchorNavAnimation enterAnim)
        {
            var element = this.CreateItem(item.Destination);

            if (index >= this.container.childCount)
            {
                this.container.Add(element);
            }
            else
            {
                this.container.Insert(index, element);
            }

            var entry = new AnchorNavActiveEntry(item.Destination, item.Arguments, item.IsPopup, item.Options, element);
            this.activeStack.Insert(index, entry);

            this.TryPlayAnimation(element, enterAnim, null);

            OnEntered(entry);
            this.EnteredDestination?.Invoke(this, element, entry.Arguments);
        }

        private void RemoveActiveEntryAt(int index, AnchorNavAnimation exitAnim)
        {
            var entry = this.activeStack[index];
            this.activeStack.RemoveAt(index);

            OnExit(entry);
            this.ExitedDestination?.Invoke(this, entry.Element, entry.Arguments);

            this.CompleteAnimationsFor(entry.Element);

            void OnCompleted()
            {
                if (entry.Element.parent != null)
                {
                    entry.Element.RemoveFromHierarchy();
                }
            }

            if (!this.TryPlayAnimation(entry.Element, exitAnim, OnCompleted))
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
                    this.runningAnimations.Remove(handleInfo);
                })
                .KeepAlive();

            handleInfo.Handle = handle;
            this.runningAnimations.Add(handleInfo);
            return true;
        }

        private static AnimationDescription GetAnimationDescription(AnchorNavAnimation animation)
        {
            return animation != null ? animation.GetDescription() : AnimationDescription.None;
        }

        private void CancelRunningAnimations()
        {
            if (this.runningAnimations.Count == 0)
            {
                return;
            }

            foreach (var animation in this.runningAnimations)
            {
                animation.CompleteImmediately();
            }

            this.runningAnimations.Clear();
        }

        private void CompleteAnimationsFor(VisualElement element)
        {
            for (var i = this.runningAnimations.Count - 1; i >= 0; i--)
            {
                var handle = this.runningAnimations[i];
                if (handle.Element != element)
                {
                    continue;
                }

                handle.CompleteImmediately();
                this.runningAnimations.RemoveAt(i);
            }
        }

        private bool TryResolveActionOrDestination(
            string actionOrDestination,
            AnchorNavArgument[] arguments,
            out string destination,
            out AnchorNavOptions options,
            out AnchorNavArgument[] mergedArguments)
        {
            destination = null;
            options = null;
            mergedArguments = arguments ?? Array.Empty<AnchorNavArgument>();

            if (string.IsNullOrWhiteSpace(actionOrDestination))
            {
                return false;
            }

            if (!this.actions.TryGetValue(actionOrDestination, out var action))
            {
                destination = actionOrDestination;
                return true;
            }

            this.ActionTriggered?.Invoke(this, action);
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

            var index = this.FindActivePopupIndex(destination);
            if (index < 0)
            {
                return false;
            }

            this.RemoveActiveEntriesFrom(index, null);
            this.UpdateCurrentFromActiveStack();
            return true;
        }

        private int FindFirstActivePopupIndex()
        {
            for (var i = this.activeStack.Count - 1; i >= 0; i--)
            {
                if (!this.activeStack[i].IsPopup)
                {
                    return i == this.activeStack.Count - 1 ? -1 : i + 1;
                }
            }

            return this.activeStack.Count > 0 ? 0 : -1;
        }

        private int FindActivePopupIndex(string destination)
        {
            for (var i = this.activeStack.Count - 1; i >= 0; i--)
            {
                var entry = this.activeStack[i];
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
            for (var i = this.activeStack.Count - 1; i >= index; i--)
            {
                this.RemoveActiveEntryAt(i, exitAnim);
            }
        }

        private void UpdateCurrentFromActiveStack()
        {
            this.TrimBackStackToActive();

            var top = this.activeStack.Count > 0 ? this.activeStack[^1] : null;
            this.CurrentDestination = top?.Destination;
            this.currentPopEnterAnimation = top?.Options.Animations.PopEnterAnim;
            this.currentPopExitAnimation = top?.Options.Animations.PopExitAnim;
        }

        private bool TryGetCurrentBase(out AnchorNavActiveEntry entry)
        {
            for (var i = this.activeStack.Count - 1; i >= 0; i--)
            {
                var candidate = this.activeStack[i];
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
            var count = Math.Min(this.activeStack.Count, targetItems.Count);
            var prefix = 0;

            while (prefix < count && this.AreEquivalent(this.activeStack[prefix], targetItems[prefix]))
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

            if (items.Count != this.activeStack.Count)
            {
                return false;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var target = items[i];
                var existing = this.activeStack[i];

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

