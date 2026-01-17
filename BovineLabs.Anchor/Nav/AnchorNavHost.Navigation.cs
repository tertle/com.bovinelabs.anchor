// <copyright file="AnchorNavHost.Navigation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Core;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.Collections;
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
        /// <param name="exitAnimation"> The animation used when removing the last active entry. </param>
        public void ClearNavigation(AnchorNavAnimation exitAnimation)
        {
            this.backStack.Clear();
            this.ApplySnapshot(AnchorNavStackSnapshot.Empty, exitAnimation, null, new AnchorNavOptions());
        }

        /// <summary> Clear the active stack and back stack so no destination remains active. </summary>
        public void ClearNavigation()
        {
            this.ClearNavigation((AnchorNavAnimation)null);
        }

        /// <summary> Clear the active stack and back stack so no destination remains active. </summary>
        /// <param name="exitAnimation"> The animation name used when removing the last active entry. </param>
        public void ClearNavigation(string exitAnimation)
        {
            this.ClearNavigation(this.ResolveAnimation(exitAnimation));
        }

        /// <summary> Clear the active stack and back stack so no destination remains active. </summary>
        /// <param name="exitAnimation"> The animation name used when removing the last active entry. </param>
        public void ClearNavigation(in FixedString32Bytes exitAnimation)
        {
            this.ClearNavigation(this.ResolveAnimation(exitAnimation));
        }

        /// <summary> Navigate to the destination with the given name. </summary>
        /// <param name="actionOrDestination"> The name of the action. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(string actionOrDestination, params Argument[] arguments)
        {
            if (this.actions.TryGetValue(actionOrDestination, out var action))
            {
                this.ActionTriggered?.Invoke(this, action);
                var options = action.Options?.Clone() ?? new AnchorNavOptions();
                var mergedArguments = action.MergeArguments(arguments);
                return this.Navigate(action.Destination, options, mergedArguments);
            }

            return this.Navigate(actionOrDestination, null, arguments);
        }

        /// <summary> Navigate to the destination with the given name. </summary>
        /// <param name="destination"> The destination. </param>
        /// <param name="options"> The options to use for the navigation. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(string destination, AnchorNavOptions options, params Argument[] arguments)
        {
            options ??= new AnchorNavOptions();

            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<Argument>();

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
        /// <param name="exitAnimation"> The animation to play when dismissing each popup. </param>
        /// <returns> True if at least one popup was closed. </returns>
        public bool CloseAllPopups(AnchorNavAnimation exitAnimation)
        {
            var removed = false;

            for (var i = this.activeStack.Count - 1; i >= 0; i--)
            {
                if (!this.activeStack[i].IsPopup)
                {
                    break;
                }

                this.RemoveActiveEntryAt(i, exitAnimation);
                removed = true;
            }

            if (!removed)
            {
                return false;
            }

            var top = this.activeStack.Count > 0 ? this.activeStack[^1] : null;
            this.CurrentDestination = top?.Destination;
            this.currentPopEnterAnimation = top?.Options.Animations.PopEnterAnim;
            this.currentPopExitAnimation = top?.Options.Animations.PopExitAnim;
            return true;
        }

        /// <summary> Close all currently-displayed popup overlays. </summary>
        public bool CloseAllPopups()
        {
            return this.CloseAllPopups((AnchorNavAnimation)null);
        }

        /// <summary> Close all currently-displayed popup overlays. </summary>
        /// <param name="exitAnimation"> The animation name to play when dismissing each popup. </param>
        /// <returns> True if at least one popup was closed. </returns>
        public bool CloseAllPopups(string exitAnimation)
        {
            return this.CloseAllPopups(this.ResolveAnimation(exitAnimation));
        }

        /// <summary> Close all currently-displayed popup overlays. </summary>
        /// <param name="exitAnimation"> The animation name to play when dismissing each popup. </param>
        /// <returns> True if at least one popup was closed. </returns>
        public bool CloseAllPopups(in FixedString32Bytes exitAnimation)
        {
            return this.CloseAllPopups(this.ResolveAnimation(exitAnimation));
        }

        /// <summary> Close a popup in the active stack that matches the provided destination. </summary>
        /// <param name="destination"> The destination key of the popup to close. </param>
        /// <param name="exitAnimation"> Optional animation to play when dismissing the popup. </param>
        /// <returns> True if the popup was closed; otherwise, false. </returns>
        public bool ClosePopup(string destination, AnchorNavAnimation exitAnimation)
        {
            if (string.IsNullOrWhiteSpace(destination) || this.activeStack.Count == 0)
            {
                return false;
            }

            var index = -1;
            for (var i = this.activeStack.Count - 1; i >= 0; i--)
            {
                var entry = this.activeStack[i];
                if (!entry.IsPopup)
                {
                    break;
                }

                if (entry.Destination == destination)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            this.RemoveActiveEntryAt(index, exitAnimation);
            this.TrimBackStackToActive();

            var top = this.activeStack.Count > 0 ? this.activeStack[^1] : null;
            this.CurrentDestination = top?.Destination;
            this.currentPopEnterAnimation = top?.Options.Animations.PopEnterAnim;
            this.currentPopExitAnimation = top?.Options.Animations.PopExitAnim;
            return true;
        }

        /// <summary> Close a popup in the active stack that matches the provided destination. </summary>
        /// <param name="destination"> The destination key of the popup to close. </param>
        /// <returns> True if the popup was closed; otherwise, false. </returns>
        public bool ClosePopup(string destination)
        {
            return this.ClosePopup(destination, (AnchorNavAnimation)null);
        }

        /// <summary> Close a popup in the active stack that matches the provided destination. </summary>
        /// <param name="destination"> The destination key of the popup to close. </param>
        /// <param name="exitAnimation"> Optional animation name to play when dismissing the popup. </param>
        /// <returns> True if the popup was closed; otherwise, false. </returns>
        public bool ClosePopup(string destination, string exitAnimation)
        {
            return this.ClosePopup(destination, this.ResolveAnimation(exitAnimation));
        }

        /// <summary> Close a popup in the active stack that matches the provided destination. </summary>
        /// <param name="destination"> The destination key of the popup to close. </param>
        /// <param name="exitAnimation"> Optional animation name to play when dismissing the popup. </param>
        /// <returns> True if the popup was closed; otherwise, false. </returns>
        public bool ClosePopup(string destination, in FixedString32Bytes exitAnimation)
        {
            return this.ClosePopup(destination, this.ResolveAnimation(exitAnimation));
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

        private void NavigateInternal(string destination, AnchorNavOptions options, Argument[] arguments)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            options ??= new AnchorNavOptions();
            arguments ??= Array.Empty<Argument>();

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

        private bool NavigatePopupInternal(string destination, AnchorNavOptions options, Argument[] arguments)
        {
            if (string.IsNullOrEmpty(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<Argument>();

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

        private bool NavigatePopupEnsureBase(string destination, AnchorNavOptions options, Argument[] arguments)
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
                var baseArgs = baseArgsList is { Count: > 0 } ? baseArgsList.ToArray() : Array.Empty<Argument>();

                if (!this.Navigate(baseDestination, baseOptions, baseArgs))
                {
                    return false;
                }
            }

            return this.NavigatePopupOnCurrent(destination, options, arguments);
        }

        private bool NavigatePopupOnCurrent(string destination, AnchorNavOptions options, Argument[] arguments)
        {
            if (this.activeStack.Count == 0)
            {
                BLGlobalLogger.LogWarning($"Popup navigation to '{destination}' requested without an active base destination. Falling back to normal navigation.");

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

        private AnchorNavAnimation ResolveAnimation(string animationName)
        {
            if (string.IsNullOrWhiteSpace(animationName))
            {
                return null;
            }

            if (this.TryGetAnimation(animationName, out var animation))
            {
                return animation;
            }

            BLGlobalLogger.LogWarning($"AnchorNavHost could not find animation '{animationName}'.");
            return null;
        }

        private AnchorNavAnimation ResolveAnimation(in FixedString32Bytes animationName)
        {
            return animationName.Length == 0 ? null : this.ResolveAnimation(animationName.ToString());
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
                this.activeStack[i].Update(targetItems[i]);
            }

            this.CancelRunningAnimations();

            for (var i = this.activeStack.Count - 1; i >= sharedPrefix; i--)
            {
                var animation = i == this.activeStack.Count - 1 ? exitAnim : null;
                this.RemoveActiveEntryAt(i, animation);
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
                BLGlobalLogger.LogWarning($"AnchorNavHost received navigation arguments for '{entry.Destination}' but no screen handled OnEnter.");
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
                BLGlobalLogger.LogWarning($"AnchorNavHost received navigation arguments for '{entry.Destination}' but no screen handled OnExit.");
            }
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
            var element = AnchorApp.current.services.GetService<IUXMLService>().Instantiate(destination);
            element.StretchToParentSize();
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }
}
