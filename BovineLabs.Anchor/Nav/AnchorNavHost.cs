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

        private readonly VisualElement _container;

        private readonly Stack<AnchorNavBackStackEntry> _backStack = new();
        private readonly Dictionary<string, AnchorNavAction> _actions = new();
        private readonly Dictionary<int, AnchorNavAnimation> _animations = new();
        private readonly List<AnchorNavActiveEntry> _activeStack = new();
        private readonly List<AnchorNavAnimationHandle> _runningAnimations = new();

        private string _currentDestination;

        private AnchorNavAnimation _currentPopExitAnimation;
        private AnchorNavAnimation _currentPopEnterAnimation;

        public AnchorNavHost(IEnumerable<AnchorAction> actions, IEnumerable<AnchorNavAnimation> animations)
            : this()
        {
            RegisterActions(actions);
            RegisterAnimations(animations);
        }

        public AnchorNavHost()
        {
            AddToClassList(USSClassName);

            style.flexGrow = 1;
            pickingMode = PickingMode.Ignore;
            _container = new VisualElement();
            _container.AddToClassList(ContainerUssClassName);
            _container.pickingMode = PickingMode.Ignore;
            _container.StretchToParentSize();
            hierarchy.Add(_container);

            RegisterAllActions();
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

        public bool CanGoBack => _backStack.Count > 0;

        public bool HasActivePopups => _activeStack.Any(e => e.IsPopup);

        /// <summary>
        /// Setting this value does not navigate or update either stack.
        /// </summary>
        public string CurrentDestination
        {
            get => _currentDestination;
            set
            {
                if (_currentDestination == value)
                {
                    return;
                }

                _currentDestination = value;
                DestinationChanged?.Invoke(this, _currentDestination);
            }
        }

        public override VisualElement contentContainer => _container.contentContainer;

        private AnchorNavBackStackEntry CurrentBackStackEntry => _backStack.TryPeek(out var entry) ? entry : null;

        public bool TryGetAnimation(int id, out AnchorNavAnimation animation)
        {
            if (id == 0)
            {
                animation = null;
                return true;
            }

            return _animations.TryGetValue(id, out animation);
        }

        private void RegisterAnimation(AnchorNavAnimation animation)
        {
            if (animation == null)
            {
                return;
            }

            if (!_animations.TryAdd(animation.ID, animation))
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

                if (_actions.ContainsKey(attribute.Name))
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

                _actions.Add(attribute.Name, action);
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

                if (_actions.ContainsKey(action.ActionName))
                {
                    BLGlobalLogger.LogError($"AnchorNavAction '{action.ActionName}' is already registered.");
                    continue;
                }

                _actions.Add(action.ActionName, action.Action);
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
                RegisterAnimation(animation);
            }
        }
    }
}
