namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class ParticleShowcasePresenter : IDisposable
    {
        private readonly AnchorParticles[] _particles;
        private readonly Button[] _buttons;
        private readonly Action[] _actions;
        private readonly VisualElement _stage;
        private readonly Label _status;
        private readonly Label _counters;
        private readonly Toggle _enabled;
        private readonly Toggle _hidden;
        private readonly Slider _speed;
        private readonly Slider _emission;
        private readonly IntegerField _seed;
        private readonly IVisualElementScheduledItem _schedule;

        public ParticleShowcasePresenter(VisualElement root, UIParticleEffect[] effects)
        {
            _stage = root.Q("particle-stage");
            _status = root.Q<Label>("particle-status");
            _counters = root.Q<Label>("particle-counters");
            _particles = new[]
            {
                root.Q<AnchorParticles>("particle-sparkle"),
                root.Q<AnchorParticles>("particle-confetti"),
                root.Q<AnchorParticles>("particle-dust"),
                root.Q<AnchorParticles>("particle-layered"),
            };

            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i].Effect = effects[i];
                _particles[i].HiddenBehaviour = UIParticleHiddenBehaviour.Pause;
            }

            _buttons = new[]
            {
                root.Q<Button>("particle-play"),
                root.Q<Button>("particle-stop"),
                root.Q<Button>("particle-pause"),
                root.Q<Button>("particle-resume"),
                root.Q<Button>("particle-clear"),
                root.Q<Button>("particle-sparkle-button"),
                root.Q<Button>("particle-overlay-button"),
            };
            _actions = new Action[]
            {
                Play,
                Stop,
                Pause,
                Resume,
                Clear,
                () => PlayAt(0, _buttons[5]),
                () => PlayAt(3, _buttons[6]),
            };

            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked += _actions[i];
            }

            _enabled = root.Q<Toggle>("particle-enabled");
            _hidden = root.Q<Toggle>("particle-hidden");
            _speed = root.Q<Slider>("particle-speed");
            _emission = root.Q<Slider>("particle-emission");
            _seed = root.Q<IntegerField>("particle-seed");
            _enabled.RegisterValueChangedCallback(EnabledChanged);
            _hidden.RegisterValueChangedCallback(HiddenChanged);
            _speed.RegisterValueChangedCallback(SpeedChanged);
            _emission.RegisterValueChangedCallback(EmissionChanged);
            _seed.RegisterValueChangedCallback(SeedChanged);
            _particles[3].Completed += Completed;
            _schedule = root.schedule.Execute(UpdateCounters).Every(100);
            UpdateCounters();
        }

        public void Dispose()
        {
            _schedule.Pause();
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked -= _actions[i];
            }

            _enabled.UnregisterValueChangedCallback(EnabledChanged);
            _hidden.UnregisterValueChangedCallback(HiddenChanged);
            _speed.UnregisterValueChangedCallback(SpeedChanged);
            _emission.UnregisterValueChangedCallback(EmissionChanged);
            _seed.UnregisterValueChangedCallback(SeedChanged);
            _particles[3].Completed -= Completed;
            foreach (var particles in _particles)
            {
                particles.Clear();
                particles.ResetSourcePoint();
            }
        }

        private void Play()
        {
            PlayAt(0, _buttons[5]);
            PlayAt(1, _particles[1]);
            PlayAt(2, _particles[2]);
            PlayAt(3, _buttons[6]);
        }

        private void PlayAt(int index, VisualElement source)
        {
            var particles = _particles[index];
            particles.SetSourcePoint(source, source.contentRect.center);
            var result = particles.Play((uint)_seed.value);
            _status.text = $"{particles.Effect.name}: {result}. Seed {_seed.value}; replay for the same burst.";
            UpdateCounters();
        }

        private void Stop()
        {
            foreach (var particles in _particles)
            {
                particles.StopEmitting();
            }

            _status.text = "Emission stopped; existing particles finish their lifetimes.";
        }

        private void Pause()
        {
            foreach (var particles in _particles)
            {
                particles.Pause();
            }

            _status.text = "Playback paused. Resume continues the same particles.";
        }

        private void Resume()
        {
            foreach (var particles in _particles)
            {
                particles.Resume();
            }

            _status.text = "Playback resumed.";
        }

        private void Clear()
        {
            foreach (var particles in _particles)
            {
                particles.Clear();
            }

            _status.text = "Particles cleared and their reserved slots released.";
            UpdateCounters();
        }

        private void EnabledChanged(ChangeEvent<bool> evt)
        {
            foreach (var particles in _particles)
            {
                particles.EffectsEnabled = evt.newValue;
            }

            _status.text = evt.newValue ? "Effects enabled. Play to start a new run." : "Effects disabled; active particles cleared.";
            UpdateCounters();
        }

        private void HiddenChanged(ChangeEvent<bool> evt)
        {
            _stage.style.visibility = evt.newValue ? Visibility.Hidden : Visibility.Visible;
            _status.text = evt.newValue ? "The hidden stage pauses simulation." : "The visible stage resumes simulation.";
        }

        private void SpeedChanged(ChangeEvent<float> evt)
        {
            foreach (var particles in _particles)
            {
                particles.PlaybackSpeed = evt.newValue;
            }
        }

        private void EmissionChanged(ChangeEvent<float> evt)
        {
            foreach (var particles in _particles)
            {
                particles.EmissionScale = evt.newValue;
            }
        }

        private void SeedChanged(ChangeEvent<int> evt)
        {
            if (evt.newValue < 0)
            {
                _seed.SetValueWithoutNotify(0);
            }
        }

        private void Completed()
        {
            _status.text = "The layered burst completed naturally. Ambient dust continues until stopped.";
        }

        private void UpdateCounters()
        {
            var live = 0;
            var reserved = 0;
            ulong emitted = 0;
            ulong dropped = 0;
            foreach (var particles in _particles)
            {
                live += particles.LiveCount;
                reserved += particles.ReservedSlots;
                emitted += particles.Counters.Emitted;
                dropped += particles.DroppedCount;
            }

            _counters.text = $"Live {live:N0}  ·  Reserved {reserved:N0}  ·  Emitted {emitted:N0}  ·  Dropped {dropped:N0}";
        }
    }
}
