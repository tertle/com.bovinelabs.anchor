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
