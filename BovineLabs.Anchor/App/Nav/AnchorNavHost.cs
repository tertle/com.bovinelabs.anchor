// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                // Scale from 1.2 to 1.0
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
                // Scale from 1.0 to 1.2
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
                // Opacity from 0.0 to 1.0
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
                // Opacity from 1.0 to 0.0v
                v.style.opacity = 1.0f - f;
            },
        };

        private readonly VisualElement container;

        private readonly Stack<AnchorNavBackStackEntry> backStack = new();
        private readonly Dictionary<Type, Stack<AnchorNavBackStackEntry>> savedStates = new();
        private readonly Dictionary<string, AnchorNavAction> actions = new();

        private Type currentDestination;
        private Argument[] currentArgs;

        private NavigationAnimation currentPopExitAnimation = NavigationAnimation.None;
        private NavigationAnimation currentPopEnterAnimation = NavigationAnimation.None;

        private ValueAnimation<float> removeAnim;
        private ValueAnimation<float> addAnim;

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

        /// <summary>
        /// Event that is invoked when an action is triggered.
        /// </summary>
        public event Action<AnchorNavHost2, AnchorNavAction> ActionTriggered;

        /// <summary>
        /// Event that is invoked when the current destination changes.
        /// </summary>
        public event Action<AnchorNavHost2, Type> DestinationChanged;

        /// <summary>
        /// Gets a value indicating whether returns true if there is a destination on the back stack that can be popped.
        /// </summary>
        public bool CanGoBack => this.backStack.Count > 0;

        /// <summary>
        /// Gets or sets the current destination.
        /// </summary>
        public Type CurrentDestination
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
        public AnchorNavBackStackEntry CurrentBackStackEntry => this.backStack.TryPeek(out var entry) ? entry : null;

        /// <summary> Gets or sets the visual controller that will be used to handle modification of Navigation visual elements, such as BottomNavBar. </summary>
        public INavVisualController VisualController { get; set; }

        /// <summary> Gets the container that will hold the current <see cref="NavigationScreen"/>. </summary>
        public override VisualElement contentContainer => this.container.contentContainer;

        /// <summary>
        /// Get the <see cref="AnimationDescription"/> for the provided <see cref="NavigationAnimation"/>.
        /// </summary>
        /// <param name="anim"> The <see cref="NavigationAnimation"/> to get the <see cref="AnimationDescription"/> for. </param>
        /// <returns> The <see cref="AnimationDescription"/> for the provided <see cref="NavigationAnimation"/>. </returns>
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

        /// <summary>
        /// Clear the back stack entirely.
        /// </summary>
        /// <remarks> This will not clear the saved states, and the current destination will not be changed. </remarks>
        public void ClearBackStack()
        {
            this.backStack.Clear();
        }

        /// <summary>
        /// Navigate to the destination with the given name.
        /// </summary>
        /// <param name="actionOrDestination"> The name of the  action. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(string route, params Argument[] arguments)
        {
            if (this.actions.TryGetValue(route, out var action))
            {
                this.ActionTriggered?.Invoke(this, action);
                return this.Navigate(action.Destination, action.Options, action.MergeArguments(arguments));
            }

            return false;
        }

        /// <summary>
        /// Navigate to the destination with the given name.
        /// </summary>
        /// <param name="destination"> The destination. </param>
        /// <param name="options"> The options to use for the navigation. </param>
        /// <param name="arguments"> The arguments to pass to the destination. </param>
        /// <returns> True if the navigation was successful. </returns>
        public bool Navigate(Type destination, AnchorNavOptions options, params Argument[] arguments)
        {
            options ??= new AnchorNavOptions();

            if (destination == null)
            {
                return false;
            }

            // if (!this.m_GraphAsset.CanNavigate(this.currentDestination, destination, route))
            //     return false;

            if (this.CurrentDestination != null)
            {
                var canPush = !options.LaunchSingleTop || this.CurrentBackStackEntry == null ||
                    this.CurrentBackStackEntry.Destination != this.CurrentDestination;

                if (canPush)
                {
                    this.backStack.Push(new AnchorNavBackStackEntry(this.CurrentDestination, options, this.currentArgs));
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

        /// <summary>
        /// Pop the current destination from the back stack and navigate to the previous destination.
        /// </summary>
        /// <returns> True if the back stack was popped, false otherwise. </returns>
        public bool PopBackStack()
        {
            if (this.backStack.Count > 0)
            {
                var entry = this.backStack.Pop();
                this.HandlePopBack(entry);
                return true;
            }

            return false;
        }

        private void NavigateInternal(Type destination, AnchorNavOptions options, Argument[] arguments)
        {
            if (destination == null)
            {
                return;
            }

            options ??= new AnchorNavOptions();
            arguments ??= Array.Empty<Argument>();

            this.HandleNavigate(new AnchorNavBackStackEntry(destination, options, arguments));
        }

        private void RestoreState(Type destination)
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

        private AnchorNavBackStackEntry PopUpTo(Type destination, bool inclusive, bool saveState)
        {
            var result = this.CurrentBackStackEntry;
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
            this.currentPopEnterAnimation = entry.Options.PopEnterAnim;
            this.currentPopExitAnimation = entry.Options.PopExitAnim;
            this.SwitchTo(entry.Destination, entry.Options.ExitAnim, entry.Options.EnterAnim, entry.Arguments, false, success =>
            {
                if (success)
                {
                    this.currentArgs = entry.Arguments;
                    this.CurrentDestination = entry.Destination;
                }
            });
        }

        private void HandlePopBack(AnchorNavBackStackEntry entry)
        {
            var dest = entry.Destination;
            var exitAnim = this.currentPopExitAnimation;
            var enterAnim = this.currentPopEnterAnimation;
            this.SwitchTo(dest, exitAnim, enterAnim, entry.Arguments, true, success =>
            {
                if (success)
                {
                    this.currentArgs = entry.Arguments;
                    this.CurrentDestination = dest;
                }
            });
        }

        /// <summary>
        /// Switch to a new <see cref="NavDestination"/> using the provided <see cref="NavigationAnimation"/>.
        /// </summary>
        /// <param name="destination"> The destination to switch to. </param>
        /// <param name="exitAnim"> The animation to use when exiting the current destination. </param>
        /// <param name="enterAnim"> The animation to use when entering the new destination. </param>
        /// <param name="args"> The arguments to pass to the new destination. </param>
        /// <param name="isPop"> Whether the navigation is a pop operation. </param>
        /// <param name="callback"> A callback that will be called when the navigation is complete. </param>
        private void SwitchTo(
            Type destination, NavigationAnimation exitAnim, NavigationAnimation enterAnim, Argument[] args, bool isPop, Action<bool> callback = null)
        {
            this.removeAnim?.Recycle();
            this.addAnim?.Recycle();

            VisualElement item;
            try
            {
                item = this.CreateItem(destination);
            }
            catch (Exception e)
            {
                Debug.LogError($"The template for navigation " + $"destination {destination.Name} could not be created: {e.Message}");
                if (e.InnerException != null)
                {
                    Debug.LogException(e.InnerException);
                }

                callback?.Invoke(false);
                return;
            }

            if (this.container.childCount == 0)
            {
                this.container.Add(item);
            }
            else
            {
                var exitAnimationFunc = GetAnimationFunc(exitAnim);
                var enterAnimationFunc = GetAnimationFunc(enterAnim);
                if (enterAnimationFunc.durationMs > 0 && exitAnimationFunc.durationMs == 0)
                {
                    exitAnimationFunc.durationMs = enterAnimationFunc.durationMs;
                }

                var previousItem = this.container[0];
                (previousItem as IAnchorNavigationScreen)?.OnExit(this, args);
                this.ExitedDestination?.Invoke(this, item, args);
                this.removeAnim = previousItem
                    .experimental
                    .animation
                    .Start(0, 1, exitAnimationFunc.durationMs, exitAnimationFunc.callback)
                    .Ease(exitAnimationFunc.easing)
                    .OnCompleted(() => this.container.Remove(previousItem))
                    .KeepAlive();

                if (isPop)
                {
                    this.container.Insert(0, item);
                }
                else
                {
                    this.container.Add(item);
                }

                this.addAnim = item
                    .experimental
                    .animation
                    .Start(0, 1, enterAnimationFunc.durationMs, enterAnimationFunc.callback)
                    .Ease(enterAnimationFunc.easing)
                    .KeepAlive();
            }

            (item as IAnchorNavigationScreen)?.OnEnter(this, args);
            this.EnteredDestination?.Invoke(this, item, args);
            callback?.Invoke(true);
        }

        /// <summary>
        /// Create a new <see cref="VisualElement"/> item based on the provided <see cref="NavDestination"/>.
        /// </summary>
        /// <param name="destination"> The destination to create the item for. </param>
        /// <returns> The created <see cref="VisualElement"/> item. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the screen type could not be created. </exception>
        private VisualElement CreateItem(Type destination)
        {
            if (AnchorApp.current.services.GetService(destination) is not VisualElement screen)
            {
                throw new InvalidOperationException($"The template '{destination}' could not be instantiated. " +
                    "Ensure that the type is a valid NavigationScreen and is accessible " + "and has a parameterless constructor.");
            }

            screen.AddToClassList("appui-navhost__item");
            return screen;
        }

        private void OnCancelNavigation(NavigationCancelEvent evt)
        {
            if (this.CanGoBack)
            {
                evt.StopPropagation();
                this.PopBackStack();
            }
        }
    }
}