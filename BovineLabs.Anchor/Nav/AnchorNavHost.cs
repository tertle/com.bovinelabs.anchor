// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    /// <summary>
    /// VisualElement that manages Anchor navigation by keeping track of the active stack, back stack, and popup overlays.
    /// </summary>
    [UxmlElement]
    public partial class AnchorNavHost : VisualElement
    {
        /// <summary> The NavHost main styling class. </summary>
        private const string USSClassName = "appui-navhost";

        /// <summary> The NavHost container styling class. </summary>
        private const string ContainerUssClassName = USSClassName + "__container";

        /// <summary> No animation. </summary>
        private static readonly AnimationDescription NoneAnimation = new()
        {
            easing = Easing.Linear,
            durationMs = 0,
            callback = null,
        };

        /// <summary> Scale down and fade in animation. </summary>
        private static readonly AnimationDescription ScaleDownFadeInAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                var delta = 1.2f - (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = f;
            },
        };

        /// <summary> Scale up and fade out animation. </summary>
        private static readonly AnimationDescription ScaleUpFadeOutAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                var delta = 1.0f + (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = 1.0f - f;
            },
        };

        /// <summary> Fade in animation. </summary>
        private static readonly AnimationDescription FadeInAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) => v.style.opacity = f,
        };

        /// <summary> Fade out animation. </summary>
        private static readonly AnimationDescription FadeOutAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) => v.style.opacity = 1.0f - f,
        };

        private readonly VisualElement container;

        private readonly Stack<AnchorNavBackStackEntry> backStack = new();
        private readonly Dictionary<string, AnchorNavAction> actions = new();
        private readonly List<AnchorNavActiveEntry> activeStack = new();
        private readonly List<AnchorNavAnimationHandle> runningAnimations = new();

        private string currentDestination;

        private NavigationAnimation currentPopExitAnimation = NavigationAnimation.None;
        private NavigationAnimation currentPopEnterAnimation = NavigationAnimation.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavHost"/> class with a predefined set of actions.
        /// </summary>
        /// <param name="actions">Named actions that can be invoked for navigation.</param>
        public AnchorNavHost(IEnumerable<AnchorNamedAction> actions)
            : this()
        {
            if (actions == null)
            {
                return;
            }

            foreach (var action in actions)
            {
                if (action == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(action.ActionName))
                {
                    BLGlobalLogger.LogError("Encountered AnchorNamedAction with an empty name.");
                    continue;
                }

                if (this.actions.ContainsKey(action.ActionName))
                {
                    BLGlobalLogger.LogError($"AnchorNavAction '{action.ActionName}' is already registered.");
                    continue;
                }

                this.actions.Add(action.ActionName, action.Action);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavHost"/> class.
        /// </summary>
        public AnchorNavHost()
        {
            this.AddToClassList(USSClassName);

            this.focusable = true;
            this.style.flexGrow = 1;
            this.pickingMode = PickingMode.Ignore;
            this.container = new StackView();
            this.container.AddToClassList(ContainerUssClassName);
            this.container.pickingMode = PickingMode.Ignore;
            this.container.StretchToParentSize();
            this.hierarchy.Add(this.container);

            this.RegisterCallback<NavigationCancelEvent>(this.OnCancelNavigation);

            this.RegisterAllActions();
        }

        /// <summary> Event that is triggered when a new destination is entered. </summary>
        public event Action<AnchorNavHost, VisualElement, Argument[]> EnteredDestination;

        /// <summary> Event that is triggered when a destination is exited. </summary>
        public event Action<AnchorNavHost, VisualElement, Argument[]> ExitedDestination;

        /// <summary> Event that is invoked when an action is triggered. </summary>
        public event Action<AnchorNavHost, AnchorNavAction> ActionTriggered;

        /// <summary> Event that is invoked when the current destination changes. </summary>
        public event Action<AnchorNavHost, string> DestinationChanged;

        /// <summary> Gets or sets a value indicating whether the navigation host can receive focus. </summary>
        public override sealed bool focusable
        {
            get => base.focusable;
            set => base.focusable = value;
        }

        /// <summary> Gets a value indicating whether returns true if there is a destination on the back stack that can be popped. </summary>
        public bool CanGoBack => this.backStack.Count > 0;

        /// <summary> Gets a value indicating whether there are popup overlays active on top of the base panel. </summary>
        public bool HasActivePopups => this.activeStack.Any(e => e.IsPopup);

        /// <summary> Gets or sets the current destination. </summary>
        public string CurrentDestination
        {
            get => this.currentDestination;
            set
            {
                if (this.currentDestination == value)
                {
                    return;
                }

                this.currentDestination = value;
                this.DestinationChanged?.Invoke(this, this.currentDestination);
            }
        }

        /// <summary> Gets the container that will hold the current <see cref="NavigationScreen"/>. </summary>
        public override VisualElement contentContainer => this.container.contentContainer;

        /// <summary> Gets the last entry on the back stack. </summary>
        private AnchorNavBackStackEntry CurrentBackStackEntry => this.backStack.TryPeek(out var entry) ? entry : null;

        private static SavedState.StackItem CreateSavedStackItem(AnchorNavActiveEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            return new SavedState.StackItem(entry.Destination, entry.Options, entry.Arguments, entry.IsPopup);
        }

        private static SavedState.StackItem CreateSavedStackItem(AnchorNavStackItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new SavedState.StackItem(item.Destination, item.Options, item.Arguments, item.IsPopup);
        }

        private static SavedState.BackStackEntry CreateSavedBackStackEntry(AnchorNavBackStackEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            var snapshotItems = entry.Snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var savedSnapshot = new List<SavedState.StackItem>(snapshotItems.Count);

            foreach (var item in snapshotItems)
            {
                var savedItem = CreateSavedStackItem(item);
                if (savedItem != null)
                {
                    savedSnapshot.Add(savedItem);
                }
            }

            return new SavedState.BackStackEntry(entry.Destination, entry.Options, entry.Arguments, savedSnapshot);
        }

        private static AnchorNavStackSnapshot CreateSnapshotFromSaved(IReadOnlyList<SavedState.StackItem> items)
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

        private static AnchorNavStackItem CreateStackItem(SavedState.StackItem item)
        {
            if (item == null)
            {
                return null;
            }

            var options = item.Options?.Clone();
            var arguments = item.Arguments?.ToArray() ?? Array.Empty<Argument>();
            return new AnchorNavStackItem(item.Destination, options, arguments, item.IsPopup);
        }

        private static AnimationDescription GetAnimationFunc(NavigationAnimation anim)
        {
            switch (anim)
            {
                case NavigationAnimation.ScaleDownFadeIn:
                    return ScaleDownFadeInAnimation;
                case NavigationAnimation.ScaleUpFadeOut:
                    return ScaleUpFadeOutAnimation;
                case NavigationAnimation.FadeIn:
                    return FadeInAnimation;
                case NavigationAnimation.FadeOut:
                    return FadeOutAnimation;
                case NavigationAnimation.None:
                default:
                    return NoneAnimation;
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

        /// <summary> Clear the back stack entirely. </summary>
        /// <remarks> The current destination remains unchanged. </remarks>
        public void ClearBackStack()
        {
            this.backStack.Clear();
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
        public bool CloseAllPopups(NavigationAnimation exitAnimation = NavigationAnimation.None)
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
            this.currentPopEnterAnimation = top?.Options.Animations.PopEnterAnim ?? NavigationAnimation.None;
            this.currentPopExitAnimation = top?.Options.Animations.PopExitAnim ?? NavigationAnimation.None;
            return true;
        }

        /// <summary>
        /// Captures the current visual stack, back stack, and popup configuration so it can be restored later.
        /// </summary>
        /// <returns>A snapshot containing the navigation state that can be supplied to <see cref="RestoreState"/>.</returns>
        public SavedState SaveState()
        {
            var activeItems = new List<SavedState.StackItem>(this.activeStack.Count);

            foreach (var entry in this.activeStack)
            {
                var savedItem = CreateSavedStackItem(entry);
                if (savedItem != null)
                {
                    activeItems.Add(savedItem);
                }
            }

            var backStackEntries = new List<SavedState.BackStackEntry>(this.backStack.Count);
            foreach (var entry in this.backStack.Reverse())
            {
                var savedEntry = CreateSavedBackStackEntry(entry);
                if (savedEntry != null)
                {
                    backStackEntries.Add(savedEntry);
                }
            }

            return new SavedState(
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
        public void RestoreState(SavedState state)
        {
            if (state == null)
            {
                return;
            }

            this.CancelRunningAnimations();

            while (this.activeStack.Count > 0)
            {
                this.RemoveActiveEntryAt(this.activeStack.Count - 1, NavigationAnimation.None);
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
                        savedEntry.Arguments?.ToArray() ?? Array.Empty<Argument>(),
                        snapshot);
                    this.backStack.Push(entry);
                }
            }

            var activeSnapshot = CreateSnapshotFromSaved(state.ActiveStack);
            var topOptions = activeSnapshot.Top?.Options;
            this.ApplySnapshot(activeSnapshot, NavigationAnimation.None, NavigationAnimation.None, topOptions);

            this.currentPopEnterAnimation = state.CurrentPopEnterAnimation;
            this.currentPopExitAnimation = state.CurrentPopExitAnimation;
            this.CurrentDestination = state.CurrentDestination;
        }

        private void RegisterAllActions()
        {
            foreach (var (method, attribute) in ReflectionUtility.GetMethodsAndAttribute<AnchorNavActionAttribute>())
            {
                if (!method.IsStatic)
                {
                    BLGlobalLogger.LogError($"AnchorNavAction method {method.DeclaringType?.FullName}.{method.Name} must be static.");
                    continue;
                }

                if (!typeof(AnchorNavAction).IsAssignableFrom(method.ReturnType))
                {
                    BLGlobalLogger.LogError($"AnchorNavAction method {method.DeclaringType?.FullName}.{method.Name} must return {nameof(AnchorNavAction)}.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    BLGlobalLogger.LogError($"AnchorNavActionAttribute on {method.DeclaringType?.FullName}.{method.Name} must define a non-empty Name.");
                    continue;
                }

                if (this.actions.ContainsKey(attribute.Name))
                {
                    BLGlobalLogger.LogError($"AnchorNavAction '{attribute.Name}' is already registered; duplicate found on {method.DeclaringType?.FullName}.{method.Name}.");
                    continue;
                }

                AnchorNavAction action;
                try
                {
                    action = (AnchorNavAction)method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    BLGlobalLogger.LogError($"Failed to invoke AnchorNavAction method {method.DeclaringType?.FullName}.{method.Name}: {ex.Message}\n{ex.StackTrace}");
                    continue;
                }

                if (action == null)
                {
                    BLGlobalLogger.LogError($"AnchorNavAction method {method.DeclaringType?.FullName}.{method.Name} returned null.");
                    continue;
                }

                this.actions.Add(attribute.Name, action);
            }
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

        private void ApplySnapshot(
            AnchorNavStackSnapshot snapshot,
            NavigationAnimation exitAnim,
            NavigationAnimation enterAnim,
            AnchorNavOptions optionsForTop)
        {
            var targetItems = snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var sharedPrefix = this.GetSharedPrefix(targetItems);

            for (var i = 0; i < sharedPrefix; i++)
            {
                this.activeStack[i].Update(targetItems[i]);
            }

            this.CancelRunningAnimations();

            for (var i = this.activeStack.Count - 1; i >= sharedPrefix; i--)
            {
                var animation = i == this.activeStack.Count - 1 ? exitAnim : NavigationAnimation.None;
                this.RemoveActiveEntryAt(i, animation);
            }

            for (var i = sharedPrefix; i < targetItems.Count; i++)
            {
                var item = targetItems[i];
                var animation = i == targetItems.Count - 1 ? enterAnim : NavigationAnimation.None;
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

        private void AddActiveEntry(int index, AnchorNavStackItem item, NavigationAnimation enterAnim)
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

        private void RemoveActiveEntryAt(int index, NavigationAnimation exitAnim)
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

        private bool TryPlayAnimation(VisualElement element, NavigationAnimation animation, Action onCompleted)
        {
            var description = GetAnimationFunc(animation);
            if (description is { durationMs: <= 0, callback: null })
            {
                onCompleted?.Invoke();
                return false;
            }

            var handleInfo = new AnchorNavAnimationHandle(element, description, onCompleted);
            var handle = element.experimental.animation
                .Start(0, 1, description.durationMs, description.callback)
                .Ease(description.easing)
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

        private VisualElement CreateItem(string destination)
        {
            var element = AnchorApp.current.services.GetService<IUXMLService>().Instantiate(destination);
            element.StretchToParentSize(); // TODO need to confirm this
            element.pickingMode = PickingMode.Ignore;
            return element;
        }

        private void OnCancelNavigation(NavigationCancelEvent evt)
        {
            if (this.CanGoBack)
            {
                evt.StopPropagation();
                this.PopBackStack();
            }
        }

        /// <summary>
        /// Serializable snapshot of an <see cref="AnchorNavHost"/> state.
        /// </summary>
        public sealed class SavedState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SavedState"/> class.
            /// </summary>
            /// <param name="currentDestination">The current top-most destination.</param>
            /// <param name="currentPopEnterAnimation">Animation that should play when re-entering after a pop.</param>
            /// <param name="currentPopExitAnimation">Animation that should play when exiting during a pop.</param>
            /// <param name="activeStack">Snapshot of the active visual stack.</param>
            /// <param name="backStack">Snapshot of the back stack entries.</param>
            public SavedState(
                string currentDestination,
                NavigationAnimation currentPopEnterAnimation,
                NavigationAnimation currentPopExitAnimation,
                IReadOnlyList<StackItem> activeStack,
                IReadOnlyList<BackStackEntry> backStack)
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
            public NavigationAnimation CurrentPopEnterAnimation { get; }

            /// <summary> Gets the animation that should play when popping the current destination. </summary>
            public NavigationAnimation CurrentPopExitAnimation { get; }

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
                public BackStackEntry(
                    string destination,
                    AnchorNavOptions options,
                    Argument[] arguments,
                    IReadOnlyList<StackItem> snapshot)
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

        private sealed class AnchorNavActiveEntry
        {
            public AnchorNavActiveEntry(
                string destination,
                Argument[] arguments,
                bool isPopup,
                AnchorNavOptions options,
                VisualElement element)
            {
                this.Destination = destination;
                this.Arguments = arguments ?? Array.Empty<Argument>();
                this.IsPopup = isPopup;
                this.Options = options ?? new AnchorNavOptions();
                this.Element = element ?? throw new ArgumentNullException(nameof(element));
            }

            public string Destination { get; }

            public bool IsPopup { get; }

            public AnchorNavOptions Options { get; private set; }

            public Argument[] Arguments { get; private set; }

            public VisualElement Element { get; }

            public void Update(AnchorNavStackItem item)
            {
                this.Options = item.Options;
                this.Arguments = item.Arguments;
            }
        }

        private sealed class AnchorNavAnimationHandle
        {
            private readonly AnimationDescription description;
            private readonly Action onCompleted;

            private bool completed;

            public AnchorNavAnimationHandle(
                VisualElement element,
                AnimationDescription description,
                Action onCompleted)
            {
                this.Element = element;
                this.description = description;
                this.onCompleted = onCompleted;
            }

            public VisualElement Element { get; }

            public ValueAnimation<float> Handle { get; set; }

            public bool TryFinalizeFromAnimation()
            {
                if (this.completed)
                {
                    return false;
                }

                this.completed = true;
                this.Handle = null;
                return true;
            }

            public void CompleteImmediately()
            {
                if (this.completed)
                {
                    return;
                }

                this.completed = true;

                this.Handle?.Recycle();
                this.Handle = null;
                this.description.callback?.Invoke(this.Element, 1f);
                this.onCompleted?.Invoke();
            }
        }
    }
}
