namespace BovineLabs.Anchor.Particles.Sample
{
    using System;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.MVVM;
    using UnityEngine;
    using UnityEngine.UIElements;

    // Create once in OnVisualGenerationInitialized; Dispose in OnVisualGenerationShuttingDown.
    public sealed class ParticleSamplePresenter : IDisposable
    {
        private readonly AnchorParticles[] _particles;
        private readonly Button[] _buttons;
        private readonly RelayCommand[] _commands;
        private readonly Toggle _enabled;
        private readonly Toggle _hide;
        private readonly VisualElement _cards;
        private readonly Label _completion;
        private bool _disposed;

        public ParticleSamplePresenter(VisualElement root)
        {
            _cards = root.Q("cards");
            _completion = root.Q<Label>("completion");
            _particles = new[]
            {
                root.Q<AnchorParticles>("sparkle"), root.Q<AnchorParticles>("confetti"),
                root.Q<AnchorParticles>("dust"), root.Q<AnchorParticles>("layered"),
            };
            _buttons = new[]
            {
                root.Q<Button>("play"), root.Q<Button>("stop"), root.Q<Button>("pause"), root.Q<Button>("resume"),
                root.Q<Button>("clear"), root.Q<Button>("step"), root.Q<Button>("sparkle-button"), root.Q<Button>("overlay-button"),
            };
            _commands = new[]
            {
                new RelayCommand(Play), new RelayCommand(Stop), new RelayCommand(Pause), new RelayCommand(Resume),
                new RelayCommand(Clear), new RelayCommand(() => Advance(1d / 60)),
                new RelayCommand(() => PlayAt(0, _buttons[6])), new RelayCommand(() => PlayAt(3, _buttons[7])),
            };
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked += _commands[i].Execute;
            }

            _enabled = root.Q<Toggle>("effects-enabled");
            _hide = root.Q<Toggle>("hide");
            _enabled.RegisterValueChangedCallback(EnabledChanged);
            _hide.RegisterValueChangedCallback(HideChanged);
            _particles[3].Completed += Completed;
        }

        public void Advance(double elapsed)
        {
            if (Application.isPlaying)
            {
                return;
            }

            foreach (var particle in _particles)
            {
                particle.Advance(elapsed);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].clicked -= _commands[i].Execute;
            }

            _enabled.UnregisterValueChangedCallback(EnabledChanged);
            _hide.UnregisterValueChangedCallback(HideChanged);
            _particles[3].Completed -= Completed;
            Clear();
            foreach (var particle in _particles)
            {
                particle.ResetSourcePoint();
            }
        }

        private void Play()
        {
            PlayAt(0, _buttons[6]);
            PlayAt(1, _particles[1]);
            PlayAt(2, _particles[2]);
            PlayAt(3, _buttons[7]);
        }

        private void PlayAt(int index, VisualElement source)
        {
            _particles[index].SetSourcePoint(source, source.contentRect.center);
            _particles[index].Play(7);
        }

        private void Stop()
        {
            foreach (var particle in _particles)
            {
                particle.StopEmitting();
            }
        }

        private void Pause()
        {
            foreach (var particle in _particles)
            {
                particle.Pause();
            }
        }

        private void Resume()
        {
            foreach (var particle in _particles)
            {
                particle.Resume();
            }
        }

        private void Clear()
        {
            foreach (var particle in _particles)
            {
                particle.Clear();
            }
        }

        private void EnabledChanged(ChangeEvent<bool> evt)
        {
            foreach (var particle in _particles)
            {
                particle.EffectsEnabled = evt.newValue;
            }
        }

        private void HideChanged(ChangeEvent<bool> evt) => _cards.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
        private void Completed() => _completion.text = "Layered burst completed naturally.";
    }
}
