namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Nav;
    using UnityEngine.UIElements;

    public sealed class AnimationShowcasePresenter : IDisposable
    {
        private readonly AnchorNavHost _nav;
        private readonly AnchorNavAnimation[] _animations;
        private readonly Dictionary<VisualElement, List<(Button Button, Action Action)>> _bindings = new();
        private readonly Toggle _scale;
        private readonly Button _reset;
        private readonly Button _transitionButton;
        private readonly VisualElement _transition;
        private readonly Label _status;
        private readonly Label _transitionStatus;
        private int _popupExit;
        private bool _transitionActive;

        public AnimationShowcasePresenter(VisualElement root, AnchorNavAnimation[] animations)
        {
            _animations = animations;
            _scale = root.Q<Toggle>("animation-scale");
            _reset = root.Q<Button>("animation-reset");
            _status = root.Q<Label>("animation-status");
            _transition = root.Q("animation-transition");
            _transitionButton = root.Q<Button>("animation-transition-button");
            _transitionStatus = root.Q<Label>("animation-transition-status");
            _nav = new AnchorNavHost(Array.Empty<AnchorAction>(), animations);
            root.Q("nav-preview").Add(_nav);
            _nav.EnteredDestination += Entered;
            _nav.ExitedDestination += Exited;
            _nav.DestinationChanged += DestinationChanged;
            _reset.clicked += Reset;
            _transitionButton.clicked += ToggleTransition;
            _transition.RegisterCallback<TransitionEndEvent>(TransitionEnded);
            Reset();
        }

        public void Dispose()
        {
            _reset.clicked -= Reset;
            _transitionButton.clicked -= ToggleTransition;
            _transition.UnregisterCallback<TransitionEndEvent>(TransitionEnded);
            _nav.ClearNavigation();
            _nav.EnteredDestination -= Entered;
            _nav.ExitedDestination -= Exited;
            _nav.DestinationChanged -= DestinationChanged;
            _nav.RemoveFromHierarchy();
        }

        private void Entered(AnchorNavHost host, VisualElement view, AnchorNavArgument[] arguments)
        {
            var bindings = new List<(Button Button, Action Action)>();
            switch (view.Q(className: "animation-route").name)
            {
                case "animation-home":
                    bindings.Add((view.Q<Button>("animation-details-button"), ShowDetails));
                    bindings.Add((view.Q<Button>("animation-home-popup-button"), ShowPopup));
                    break;
                case "animation-details":
                    bindings.Add((view.Q<Button>("animation-back-button"), Back));
                    bindings.Add((view.Q<Button>("animation-details-popup-button"), ShowPopup));
                    break;
                case "animation-popup":
                    bindings.Add((view.Q<Button>("animation-close-button"), ClosePopup));
                    break;
            }

            foreach (var (button, action) in bindings)
            {
                button.clicked += action;
            }

            _bindings.Add(view, bindings);
        }

        private void Exited(AnchorNavHost host, VisualElement view, AnchorNavArgument[] arguments)
        {
            foreach (var (button, action) in _bindings[view])
            {
                button.clicked -= action;
            }

            _bindings.Remove(view);
        }

        private AnchorNavOptions Options()
        {
            var offset = _scale.value ? 2 : 0;
            return new AnchorNavOptions
            {
                Animations = new AnchorAnimations
                {
                    EnterAnim = _animations[offset],
                    ExitAnim = _animations[offset + 1],
                    PopEnterAnim = _animations[offset],
                    PopExitAnim = _animations[offset + 1],
                },
            };
        }

        private void ShowDetails()
        {
            _nav.Navigate("animation-details", Options());
        }

        private void ShowPopup()
        {
            var options = Options();
            options.PopupStrategy = AnchorPopupStrategy.PopupOnCurrent;
            options.PopupExistingStrategy = AnchorPopupExistingStrategy.CloseOtherPopups;
            _popupExit = options.Animations.ExitAnim.ID;
            _nav.Navigate("animation-popup", options);
        }

        private void Back()
        {
            _nav.PopBackStack();
        }

        private void ClosePopup()
        {
            _nav.ClosePopup("animation-popup", _popupExit);
        }

        private void Reset()
        {
            _nav.ClearNavigation();
            _nav.Navigate("animation-home", new AnchorNavOptions());
        }

        private void DestinationChanged(AnchorNavHost host, string destination)
        {
            _status.text = $"Route: {destination ?? "none"}  ·  Back available: {host.CanGoBack}  ·  Popup active: {host.HasActivePopups}";
        }

        private void ToggleTransition()
        {
            _transitionActive = !_transitionActive;
            _transition.EnableInClassList("transition-card--active", _transitionActive);
            _transitionButton.text = _transitionActive ? "Remove active class" : "Apply active class";
            _transitionStatus.text = "USS animates the changed opacity and background color.";
        }

        private void TransitionEnded(TransitionEndEvent evt)
        {
            _transitionStatus.text = "TransitionEndEvent received. Toggle again to reverse the transition.";
        }
    }
}
