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
        private readonly AnchorParticles[] particles;
        private readonly Button[] buttons;
        private readonly RelayCommand[] commands;
        private readonly Toggle enabled;
        private readonly Toggle hide;
        private readonly VisualElement cards;
        private readonly Label completion;
        private bool disposed;

        public ParticleSamplePresenter(VisualElement root)
        {
            this.cards = root.Q("cards");
            this.completion = root.Q<Label>("completion");
            this.particles = new[]
            {
                root.Q<AnchorParticles>("sparkle"), root.Q<AnchorParticles>("confetti"),
                root.Q<AnchorParticles>("dust"), root.Q<AnchorParticles>("layered"),
            };
            this.buttons = new[]
            {
                root.Q<Button>("play"), root.Q<Button>("stop"), root.Q<Button>("pause"), root.Q<Button>("resume"),
                root.Q<Button>("clear"), root.Q<Button>("step"), root.Q<Button>("sparkle-button"), root.Q<Button>("overlay-button"),
            };
            this.commands = new[]
            {
                new RelayCommand(this.Play), new RelayCommand(this.Stop), new RelayCommand(this.Pause), new RelayCommand(this.Resume),
                new RelayCommand(this.Clear), new RelayCommand(() => this.Advance(1d / 60)),
                new RelayCommand(() => this.PlayAt(0, this.buttons[6])), new RelayCommand(() => this.PlayAt(3, this.buttons[7])),
            };
            for (var i = 0; i < this.buttons.Length; i++)
            {
                this.buttons[i].clicked += this.commands[i].Execute;
            }

            this.enabled = root.Q<Toggle>("effects-enabled");
            this.hide = root.Q<Toggle>("hide");
            this.enabled.RegisterValueChangedCallback(this.EnabledChanged);
            this.hide.RegisterValueChangedCallback(this.HideChanged);
            this.particles[3].Completed += this.Completed;
        }

        public void Advance(double elapsed)
        {
            if (Application.isPlaying)
            {
                return;
            }

            foreach (var particle in this.particles)
            {
                particle.Advance(elapsed);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            for (var i = 0; i < this.buttons.Length; i++)
            {
                this.buttons[i].clicked -= this.commands[i].Execute;
            }

            this.enabled.UnregisterValueChangedCallback(this.EnabledChanged);
            this.hide.UnregisterValueChangedCallback(this.HideChanged);
            this.particles[3].Completed -= this.Completed;
            this.Clear();
            foreach (var particle in this.particles)
            {
                particle.ResetSourcePoint();
            }
        }

        private void Play()
        {
            this.PlayAt(0, this.buttons[6]);
            this.PlayAt(1, this.particles[1]);
            this.PlayAt(2, this.particles[2]);
            this.PlayAt(3, this.buttons[7]);
        }

        private void PlayAt(int index, VisualElement source)
        {
            this.particles[index].SetSourcePoint(source, source.contentRect.center);
            this.particles[index].Play(7);
        }

        private void Stop()
        {
            foreach (var particle in this.particles)
            {
                particle.StopEmitting();
            }
        }

        private void Pause()
        {
            foreach (var particle in this.particles)
            {
                particle.Pause();
            }
        }

        private void Resume()
        {
            foreach (var particle in this.particles)
            {
                particle.Resume();
            }
        }

        private void Clear()
        {
            foreach (var particle in this.particles)
            {
                particle.Clear();
            }
        }

        private void EnabledChanged(ChangeEvent<bool> evt)
        {
            foreach (var particle in this.particles)
            {
                particle.EffectsEnabled = evt.newValue;
            }
        }

        private void HideChanged(ChangeEvent<bool> evt) => this.cards.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
        private void Completed() => this.completion.text = "Layered burst completed naturally.";
    }
}
