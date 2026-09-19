namespace BovineLabs.Anchor.Particles
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using BovineLabs.Anchor.Elements;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class UIParticleCoordinator
    {
        [NoAutoStaticsCleanup]
        private static readonly ConditionalWeakTable<IPanel, UIParticleCoordinator> Coordinators = new();
        private readonly List<AnchorParticles> elements = new();
        private readonly IPanel panel;
        private readonly IVisualElementScheduledItem scheduled;
        private int lastFrame = -1;

        private UIParticleCoordinator(IPanel panel)
        {
            this.panel = panel;
            this.scheduled = panel.visualTree.schedule.Execute(this.Update).Every(1);
        }

        internal static UIParticleCoordinator Register(IPanel panel, AnchorParticles element)
        {
            if (!Coordinators.TryGetValue(panel, out var coordinator))
            {
                coordinator = new UIParticleCoordinator(panel);
                Coordinators.Add(panel, coordinator);
            }

            coordinator.elements.Add(element);
            return coordinator;
        }

        internal void Unregister(AnchorParticles element)
        {
            this.elements.Remove(element);
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
            var elapsed = (double)Time.unscaledDeltaTime;
            foreach (var element in this.elements)
            {
                element.Advance(elapsed);
            }
        }
    }
}
