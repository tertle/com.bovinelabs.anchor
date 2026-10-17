namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class RewardFlightShowcasePresenter : IDisposable
    {
        private readonly VisualElement _root;
        private readonly VisualElement _targetParent;
        private readonly VisualElement _overlay;
        private readonly Label _counter;
        private readonly Label _status;
        private readonly AnchorParticles[] _particles;
        private readonly Button[] _buttons;
        private readonly Action[] _actions;
        private readonly Toggle _follow;
        private readonly Toggle _enabled;
        private readonly Toggle _hidden;
        private readonly IVisualElementScheduledItem _pulse;
        private int _value;
        private bool _moved;

        public RewardFlightShowcasePresenter(VisualElement root, UIParticleEffect snapshot, UIParticleEffect follow)
        {
            _root = root;
            _targetParent = root.Q("reward-target-parent");
            _overlay = root.Q("reward-overlay");
            _counter = root.Q<Label>("reward-counter");
            _status = root.Q<Label>("reward-status");
            _follow = root.Q<Toggle>("reward-follow");
            _enabled = root.Q<Toggle>("reward-enabled");
            _hidden = root.Q<Toggle>("reward-hidden");
            _particles = new[]
            {
                root.Q<AnchorParticles>("reward-snapshot"),
                root.Q<AnchorParticles>("reward-following"),
            };
            _particles[0].Effect = snapshot;
            _particles[1].Effect = follow;
            foreach (var particles in _particles)
            {
                particles.RefreshTarget(root);
                particles.TargetPoint = new Vector2(75, 30);
                particles.Arrived += Arrived;
            }

            _buttons = new[]
            {
                root.Q<Button>("reward-source-0"),
                root.Q<Button>("reward-source-1"),
                root.Q<Button>("reward-source-2"),
                root.Q<Button>("reward-pause"),
                root.Q<Button>("reward-resume"),
                root.Q<Button>("reward-move"),
                root.Q<Button>("reward-remove"),
            };
            _actions = new Action[]
            {
                () => Collect(0, 5),
                () => Collect(1, 10),
                () => Collect(2, 20),
                () => {
                    foreach (var particles in _particles) particles.Pause();
                },
                () => {
                    foreach (var particles in _particles) particles.Resume();
                },
                MoveTarget,
                ToggleTarget,
            };
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked += _actions[i];
            }

            _follow.RegisterValueChangedCallback(ModeChanged);
            _enabled.RegisterValueChangedCallback(EnabledChanged);
            _hidden.RegisterValueChangedCallback(HiddenChanged);
            _pulse = _counter.schedule.Execute(() => _counter.RemoveFromClassList("is-pulsing"));
            _pulse.Pause();
        }

        public void Dispose()
        {
            _pulse.Pause();
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked -= _actions[i];
            }

            _follow.UnregisterValueChangedCallback(ModeChanged);
            _enabled.UnregisterValueChangedCallback(EnabledChanged);
            _hidden.UnregisterValueChangedCallback(HiddenChanged);
            foreach (var particles in _particles)
            {
                particles.Arrived -= Arrived;
                particles.Target = null;
                particles.Clear();
            }
        }

        private void Collect(int sourceIndex, int count)
        {
            // The command owns the value. Suppressed, cancelled and dropped visuals cannot alter it.
            _value = checked(_value + count);
            _counter.text = $"{_value} coins";
            var source = _buttons[sourceIndex];
            var result = _particles[_follow.value ? 1 : 0].Emit(0, count, source, source.contentRect.center);
            _status.text = $"Counter updated immediately. {result.Status}: {result.Emitted} visuals admitted, {result.Dropped} dropped.";
        }

        private void Arrived(int count)
        {
            _counter.AddToClassList("is-pulsing");
            _pulse.ExecuteLater(140);
        }

        private void ModeChanged(ChangeEvent<bool> evt)
        {
            foreach (var particles in _particles)
            {
                particles.Clear();
            }

            _status.text = evt.newValue ? "Follow selected; moving the target reshapes the remaining curve." :
                "Snapshot selected; each destination is captured when its scatter ends.";
        }

        private void EnabledChanged(ChangeEvent<bool> evt)
        {
            foreach (var particles in _particles)
            {
                particles.EffectsEnabled = evt.newValue;
            }
        }

        private void HiddenChanged(ChangeEvent<bool> evt) => _overlay.style.visibility = evt.newValue ? Visibility.Hidden : Visibility.Visible;

        private void MoveTarget()
        {
            _moved = !_moved;
            _targetParent.EnableInClassList("is-moved", _moved);
        }

        private void ToggleTarget()
        {
            if (_counter.parent != null)
            {
                _counter.RemoveFromHierarchy();
                _status.text = "Target removed; active flights cancel without arrivals or completion.";
                return;
            }

            _targetParent.Add(_counter);
            foreach (var particles in _particles)
            {
                particles.RefreshTarget(_root);
            }

            _status.text = "Target restored and explicitly rebound. Collect again to start a new run.";
        }
    }
}
