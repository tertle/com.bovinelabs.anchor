namespace BovineLabs.Anchor.Particles.Sample
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.Profiling;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    // Experimental panel ownership/update hook, deliberately confined to the sample.
    public sealed class ParticleFixtureClock
    {
        // Weak keys cannot retain a discarded panel across a visual or play-mode generation.
        [NoAutoStaticsCleanup]
        private static readonly ConditionalWeakTable<IPanel, ParticleFixtureClock> Clocks = new();
        private static readonly ProfilerMarker TickMarker = new("Anchor.Particles.PanelTick");
        private readonly List<ParticleFixtureElement> elements = new();
        private readonly IPanel panel;
        private readonly IVisualElementScheduledItem scheduled;
        private int lastFrame = -1;

        private ParticleFixtureClock(IPanel panel)
        {
            this.panel = panel;
            this.scheduled = panel.visualTree.schedule.Execute(this.Update).Every(1);
        }

        public static ParticleFixtureClock Register(IPanel panel, ParticleFixtureElement element)
        {
            if (!Clocks.TryGetValue(panel, out var clock))
            {
                clock = new ParticleFixtureClock(panel);
                Clocks.Add(panel, clock);
            }

            clock.elements.Add(element);
            return clock;
        }

        public void Unregister(ParticleFixtureElement element)
        {
            this.elements.Remove(element);
            if (this.elements.Count == 0)
            {
                this.scheduled.Pause();
                Clocks.Remove(this.panel);
            }
        }

        public static void PreviewStep(IPanel panel, int frame)
        {
            if (Clocks.TryGetValue(panel, out var clock))
            {
                clock.Advance(frame);
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                this.Advance(Time.frameCount);
            }
        }

        private void Advance(int frame)
        {
            if (this.lastFrame == frame)
            {
                return;
            }

            this.lastFrame = frame;
            using (TickMarker.Auto())
            {
                foreach (var element in this.elements)
                {
                    element.Tick();
                }
            }
        }
    }
}
