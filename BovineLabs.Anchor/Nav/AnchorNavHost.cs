// <copyright file="AnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

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

        private readonly VisualElement container;

        private readonly Stack<AnchorNavBackStackEntry> backStack = new();
        private readonly Dictionary<string, AnchorNavAction> actions = new();
        private readonly Dictionary<string, AnchorNavAnimation> animations = new();
        private readonly List<AnchorNavActiveEntry> activeStack = new();
        private readonly List<AnchorNavAnimationHandle> runningAnimations = new();

        private string currentDestination;

        private AnchorNavAnimation currentPopExitAnimation;
        private AnchorNavAnimation currentPopEnterAnimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavHost"/> class with predefined actions and animations.
        /// </summary>
        /// <param name="actions">Named actions that can be invoked for navigation.</param>
        /// <param name="animations">Named animations that can be invoked by key.</param>
        public AnchorNavHost(IEnumerable<AnchorNamedAction> actions, IEnumerable<AnchorNavAnimation> animations)
            : this()
        {
            this.RegisterActions(actions);
            this.RegisterAnimations(animations);
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

        /// <summary>
        /// Register an animation by name for lookup during navigation.
        /// </summary>
        /// <param name="animationName">The animation name.</param>
        /// <param name="animation">The animation definition.</param>
        public void RegisterAnimation(string animationName, AnchorNavAnimation animation)
        {
            if (string.IsNullOrWhiteSpace(animationName))
            {
                BLGlobalLogger.LogError("Encountered AnchorNamedAnimation with an empty name.");
                return;
            }

            if (animation == null)
            {
                BLGlobalLogger.LogError($"AnchorNavAnimation '{animationName}' is null.");
                return;
            }

            if (!this.animations.TryAdd(animationName, animation))
            {
                BLGlobalLogger.LogError($"AnchorNavAnimation '{animationName}' is already registered.");
            }
        }

        /// <summary>
        /// Try to resolve a registered animation by name.
        /// </summary>
        /// <param name="name">The animation name.</param>
        /// <param name="animation">The animation definition.</param>
        /// <returns>True if the animation was found.</returns>
        public bool TryGetAnimation(string name, out AnchorNavAnimation animation)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                animation = null;
                return false;
            }

            return this.animations.TryGetValue(name, out animation);
        }

        /// <summary>
        /// Resolve a registered animation by name.
        /// </summary>
        /// <param name="name">The animation name.</param>
        /// <returns>The animation definition, or null if not found.</returns>
        public AnchorNavAnimation GetAnimation(string name)
        {
            this.TryGetAnimation(name, out var animation);
            return animation;
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

        private void RegisterActions(IEnumerable<AnchorNamedAction> allActions)
        {
            if (allActions == null)
            {
                return;
            }

            foreach (var action in allActions)
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

        private void RegisterAnimations(IEnumerable<AnchorNavAnimation> allAnimations)
        {
            if (allAnimations == null)
            {
                return;
            }

            foreach (var animation in allAnimations)
            {
                if (animation == null)
                {
                    continue;
                }

                this.RegisterAnimation(animation.name, animation);
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
    }
}
