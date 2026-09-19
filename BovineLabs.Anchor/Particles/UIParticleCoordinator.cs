namespace BovineLabs.Anchor.Particles
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Elements;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class UIParticleCoordinator
    {
        [NoAutoStaticsCleanup]
        private static readonly Dictionary<IPanel, UIParticleCoordinator> Coordinators = new();
        private readonly List<AnchorParticles> elements = new();
        private readonly List<AnchorParticles> notifications = new();
        private readonly IPanel panel;
        private readonly IVisualElementScheduledItem scheduled;
        private int lastFrame = -1;

        private UIParticleCoordinator(IPanel panel)
        {
            this.panel = panel;
            this.scheduled = panel.visualTree.schedule.Execute(this.Update).Every(1);
            this.scheduled.Pause();
        }

        internal static UIParticleCoordinator Register(IPanel panel, AnchorParticles element)
        {
            if (!Coordinators.TryGetValue(panel, out var coordinator))
            {
                coordinator = new UIParticleCoordinator(panel);
                Coordinators.Add(panel, coordinator);
            }

            if (!coordinator.elements.Contains(element))
            {
                coordinator.elements.Add(element);
            }

            return coordinator;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            var coordinators = new List<UIParticleCoordinator>(Coordinators.Values);
            foreach (var coordinator in coordinators)
            {
                coordinator.scheduled.Pause();
                foreach (var element in coordinator.elements.ToArray())
                {
                    element.ReleaseVisualGeneration();
                }

                coordinator.elements.Clear();
                coordinator.notifications.Clear();
            }

            Coordinators.Clear();
        }

        internal void Wake()
        {
            if (Application.isPlaying)
            {
                this.scheduled.Resume();
            }
        }

        internal void Unregister(AnchorParticles element)
        {
            if (!this.elements.Remove(element))
            {
                return;
            }

            if (this.elements.Count == 0)
            {
                this.scheduled.Pause();
                Coordinators.Remove(this.panel);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || this.lastFrame == Time.frameCount)
            {
                return;
            }

            this.lastFrame = Time.frameCount;
            var unscaled = (double)Time.unscaledDeltaTime;
            var scaled = (double)Time.deltaTime;
            foreach (var element in this.elements)
            {
                if (!element.ManualClock && element.NeedsUpdate)
                {
                    element.Tick(element.TimeMode == UIParticleTimeMode.Scaled ? scaled : unscaled);
                    this.notifications.Add(element);
                }
            }

            // Handlers may detach controls, replace assets, or restart this or another run.
            for (var i = 0; i < this.notifications.Count; i++)
            {
                this.notifications[i].DispatchCompletion();
            }

            this.notifications.Clear();
            foreach (var element in this.elements)
            {
                if (!element.ManualClock && element.NeedsUpdate)
                {
                    return;
                }
            }

            this.scheduled.Pause();
        }
    }
}
