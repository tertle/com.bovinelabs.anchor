namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorNavHost : VisualElement, IAnchorNavHost
    {
        private const string USSClassName = "appui-navhost";

        private const string ContainerUssClassName = USSClassName + "__container";

        private readonly VisualElement container;

        private readonly Stack<AnchorNavBackStackEntry> backStack = new();
        private readonly Dictionary<string, AnchorNavAction> actions = new();
        private readonly Dictionary<int, AnchorNavAnimation> animations = new();
        private readonly List<AnchorNavActiveEntry> activeStack = new();
        private readonly List<AnchorNavAnimationHandle> runningAnimations = new();

        private string currentDestination;

        private AnchorNavAnimation currentPopExitAnimation;
        private AnchorNavAnimation currentPopEnterAnimation;

        public AnchorNavHost(IEnumerable<AnchorAction> actions, IEnumerable<AnchorNavAnimation> animations)
            : this()
        {
            this.RegisterActions(actions);
            this.RegisterAnimations(animations);
        }

        public AnchorNavHost()
        {
            this.AddToClassList(USSClassName);

            this.style.flexGrow = 1;
            this.pickingMode = PickingMode.Ignore;
            this.container = new VisualElement();
            this.container.AddToClassList(ContainerUssClassName);
            this.container.pickingMode = PickingMode.Ignore;
            this.container.StretchToParentSize();
            this.hierarchy.Add(this.container);

            this.RegisterAllActions();
        }

        public event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> EnteredDestination;

        public event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> ExitedDestination;

        public event Action<AnchorNavHost, AnchorNavAction> ActionTriggered;

        public event Action<AnchorNavHost, string> DestinationChanged;

        public override sealed bool focusable
        {
            get => base.focusable;
            set => base.focusable = value;
        }

        public bool CanGoBack => this.backStack.Count > 0;

        public bool HasActivePopups => this.activeStack.Any(e => e.IsPopup);

        /// <summary>
        /// Setting this value does not navigate or update either stack.
        /// </summary>
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

        public override VisualElement contentContainer => this.container.contentContainer;

        private AnchorNavBackStackEntry CurrentBackStackEntry => this.backStack.TryPeek(out var entry) ? entry : null;

        public bool TryGetAnimation(int id, out AnchorNavAnimation animation)
        {
            if (id == 0)
            {
                animation = null;
                return true;
            }

            return this.animations.TryGetValue(id, out animation);
        }

        private void RegisterAnimation(AnchorNavAnimation animation)
        {
            if (animation == null)
            {
                return;
            }

            if (!this.animations.TryAdd(animation.ID, animation))
            {
                BLGlobalLogger.LogError($"AnchorNavAnimation id {animation.ID} on '{animation.name}' is already registered.");
            }
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

        private void RegisterActions(IEnumerable<AnchorAction> allActions)
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
                this.RegisterAnimation(animation);
            }
        }
    }
}
