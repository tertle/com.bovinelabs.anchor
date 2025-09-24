// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    public class AnchorNavHost : NavHost
    {
        public AnchorNavHost()
        {
            this.pickingMode = PickingMode.Ignore;
            this.Q<StackView>().pickingMode = PickingMode.Ignore;
        }
    }

    [UxmlElement]
    public partial class AnchorNavHost2 : VisualElement
    {
        /// <summary> The NavHost main styling class. </summary>
        public const string USSClassName = "appui-navhost";

        /// <summary> The NavHost container styling class. </summary>
        public const string ContainerUssClassName = USSClassName + "__container";

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
            callback = (v, f) =>
            {
                v.style.opacity = f;
            },
        };

        /// <summary> Fade out animation. </summary>
        private static readonly AnimationDescription FadeOutAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) =>
            {
                v.style.opacity = 1.0f - f;
            },
        };

        private readonly VisualElement container;

        private readonly Stack<AnchorNavBackStackEntry> backStack = new();
        private readonly Dictionary<string, Stack<AnchorNavBackStackEntry>> savedStates = new();
        private readonly Dictionary<string, AnchorNavAction> actions = new();
        private readonly List<AnchorNavActiveEntry> activeStack = new();
        private readonly List<AnchorNavAnimationHandle> runningAnimations = new();

        private string currentDestination;
        private Argument[] currentArgs = Array.Empty<Argument>();

        private NavigationAnimation currentPopExitAnimation = NavigationAnimation.None;
        private NavigationAnimation currentPopEnterAnimation = NavigationAnimation.None;

        public AnchorNavHost2()
        {
            this.AddToClassList(USSClassName);

            this.focusable = true;
            this.pickingMode = PickingMode.Ignore;
            this.container = new StackView();
            this.container.AddToClassList(ContainerUssClassName);
            this.container.pickingMode = PickingMode.Ignore;
            this.container.StretchToParentSize();
            this.hierarchy.Add(this.container);

            this.RegisterCallback<NavigationCancelEvent>(this.OnCancelNavigation);
        }

        /// <summary> Event that is triggered when a new destination is entered. </summary>
        public event Action<AnchorNavHost2, VisualElement, Argument[]> EnteredDestination;

        /// <summary> Event that is triggered when a destination is exited. </summary>
        public event Action<AnchorNavHost2, VisualElement, Argument[]> ExitedDestination;

        /// <summary> Event that is invoked when an action is triggered. </summary>
        public event Action<AnchorNavHost2, AnchorNavAction> ActionTriggered;

        /// <summary> Event that is invoked when the current destination changes. </summary>
        public event Action<AnchorNavHost2, string> DestinationChanged;

        /// <inheritdoc/>
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

        /// <summary> Gets the last entry on the back stack. </summary>
        private AnchorNavBackStackEntry CurrentBackStackEntry => this.backStack.TryPeek(out var entry) ? entry : null;

        /// <summary> Gets or sets the visual controller that will be used to handle modification of Navigation visual elements, such as BottomNavBar. </summary>
        public INavVisualController VisualController { get; set; }

        /// <summary> Gets the container that will hold the current <see cref="NavigationScreen"/>. </summary>
        public override VisualElement contentContainer => this.container.contentContainer;

        /// <summary> Clear the back stack entirely. </summary>
        /// <remarks> This will not clear the saved states, and the current destination will not be changed. </remarks>
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
                return this.Navigate(action.Destination, action.Options, action.MergeArguments(arguments));
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

            if (destination == null)
            {
                return false;
            }

            arguments ??= Array.Empty<Argument>();

            if (this.activeStack.Count > 0)
            {
                var currentSnapshot = this.CaptureCurrentSnapshot();
                var canPush = !options.LaunchSingleTop || this.CurrentBackStackEntry == null ||
                    this.CurrentBackStackEntry.Destination != this.CurrentDestination;

                if (canPush)
                {
                    this.PushSnapshot(currentSnapshot);
                }
            }

            if (options.StackStrategy == AnchorStackStrategy.PopToRoot && this.backStack.Count > 0)
            {
                options.PopUpToDestination = this.backStack.ElementAt(this.backStack.Count - 1).Destination;
            }

            if (options.StackStrategy != AnchorStackStrategy.None && options.PopUpToDestination != null)
            {
                this.PopUpTo(options.PopUpToDestination, options.PopUpToInclusive, options.PopUpToSaveState);
            }

            if (options.RestoreState)
            {
                this.RestoreState(destination);
            }

            if (destination == this.CurrentBackStackEntry?.Destination && options.LaunchSingleTop)
            {
                this.backStack.Pop();
            }

            this.NavigateInternal(destination, options, arguments);
            return true;
        }

        /// <summary> Display a popup destination on top of the existing stack. </summary>
        /// <param name="destination"> The popup destination. </param>
        /// <param name="options"> The options to use for the popup. </param>
        /// <param name="arguments"> The arguments to pass to the popup. </param>
        /// <returns> True if the popup navigation was successful. </returns>
        public bool Popup(string destination, AnchorNavOptions options = null, params Argument[] arguments)
        {
            options ??= new AnchorNavOptions();

            if (string.IsNullOrEmpty(destination))
            {
                return false;
            }

            arguments ??= Array.Empty<Argument>();

            if (this.activeStack.Count == 0)
            {
                return this.Navigate(destination, options, arguments);
            }

            var currentSnapshot = this.CaptureCurrentSnapshot();
            var topEntry = this.activeStack[^1];

            if (options.LaunchSingleTop && topEntry.Destination == destination && topEntry.IsPopup)
            {
                var updatedItems = currentSnapshot.Items.ToList();
                updatedItems[^1] = new AnchorNavStackItem(destination, options, arguments, true);
                var updatedSnapshot = new AnchorNavStackSnapshot(updatedItems);
                this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, updatedSnapshot));
                return true;
            }

            this.PushSnapshot(currentSnapshot);

            var items = currentSnapshot.Items.ToList();
            items.Add(new AnchorNavStackItem(destination, options, arguments, true));
            var targetSnapshot = new AnchorNavStackSnapshot(items);

            this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, targetSnapshot));
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
            this.currentArgs = top?.Arguments ?? Array.Empty<Argument>();
            this.CurrentDestination = top?.Destination;
            this.currentPopEnterAnimation = top?.Options?.PopEnterAnim ?? NavigationAnimation.None;
            this.currentPopExitAnimation = top?.Options?.PopExitAnim ?? NavigationAnimation.None;
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

        private void NavigateInternal(string destination, AnchorNavOptions options, Argument[] arguments)
        {
            if (destination == null)
            {
                return;
            }

            options ??= new AnchorNavOptions();
            arguments ??= Array.Empty<Argument>();

            var snapshot = new AnchorNavStackSnapshot(new[]
            {
                new AnchorNavStackItem(destination, options, arguments, false),
            });

            this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments, snapshot));
        }

        private void RestoreState(string destination)
        {
            if (this.savedStates.TryGetValue(destination, out var stack))
            {
                while (stack.TryPop(out var entry))
                {
                    this.backStack.Push(entry);
                }

                this.savedStates.Remove(destination);
            }
        }

        private AnchorNavBackStackEntry PopUpTo(string destination, bool inclusive, bool saveState)
        {
            AnchorNavBackStackEntry result = this.CurrentBackStackEntry;
            if (saveState)
            {
                if (this.savedStates.TryGetValue(destination, out var state))
                {
                    state.Clear();
                }
                else
                {
                    this.savedStates[destination] = new Stack<AnchorNavBackStackEntry>();
                }
            }

            while (this.backStack.TryPeek(out var entry))
            {
                if (entry.Destination == destination)
                {
                    if (inclusive)
                    {
                        result = this.backStack.Pop();
                        if (saveState)
                        {
                            this.savedStates[destination].Push(result);
                        }
                    }

                    break;
                }

                result = this.backStack.Pop();
                if (saveState)
                {
                    this.savedStates[destination].Push(result);
                }
            }

            return result;
        }

        private void HandleNavigate(AnchorNavBackStackEntry entry)
        {
            this.currentPopEnterAnimation = entry.Options?.PopEnterAnim ?? NavigationAnimation.None;
            this.currentPopExitAnimation = entry.Options?.PopExitAnim ?? NavigationAnimation.None;

            this.ApplySnapshot(
                entry.Snapshot,
                entry.Options?.ExitAnim ?? NavigationAnimation.None,
                entry.Options?.EnterAnim ?? NavigationAnimation.None,
                entry.Options);

            this.currentArgs = entry.Arguments ?? Array.Empty<Argument>();
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

            this.currentArgs = entry.Arguments ?? Array.Empty<Argument>();
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
            this.currentArgs = top?.Arguments ?? Array.Empty<Argument>();
            this.CurrentDestination = top?.Destination;

            this.currentPopEnterAnimation = optionsForTop?.PopEnterAnim ?? NavigationAnimation.None;
            this.currentPopExitAnimation = optionsForTop?.PopExitAnim ?? NavigationAnimation.None;
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
            if (snapshot == null || snapshot.Top == null)
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

            (element as IAnchorNavigationScreen)?.OnEnter(this, entry.Arguments);
            this.EnteredDestination?.Invoke(this, element, entry.Arguments);
        }

        private void RemoveActiveEntryAt(int index, NavigationAnimation exitAnim)
        {
            var entry = this.activeStack[index];
            this.activeStack.RemoveAt(index);

            (entry.Element as IAnchorNavigationScreen)?.OnExit(this, entry.Arguments);
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
            if (description.durationMs <= 0 && description.callback == null)
            {
                onCompleted?.Invoke();
                return false;
            }

            var handleInfo = new AnchorNavAnimationHandle(element, description, onCompleted);
            ValueAnimation<float> handle = null;
            handle = element.experimental.animation
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
            var service = (IUXMLService)AnchorApp.current.services.GetService(typeof(IUXMLService));
            return service.Instantiate(destination);
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

        private void OnCancelNavigation(NavigationCancelEvent evt)
        {
            if (this.CanGoBack)
            {
                evt.StopPropagation();
                this.PopBackStack();
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
            private bool completed;

            public AnchorNavAnimationHandle(
                VisualElement element,
                AnimationDescription description,
                Action onCompleted)
            {
                this.Element = element;
                this.Description = description;
                this.OnCompleted = onCompleted;
            }

            public VisualElement Element { get; }

            public AnimationDescription Description { get; }

            public Action OnCompleted { get; }

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
                this.Description.callback?.Invoke(this.Element, 1f);
                this.OnCompleted?.Invoke();
            }
        }
    }
}
