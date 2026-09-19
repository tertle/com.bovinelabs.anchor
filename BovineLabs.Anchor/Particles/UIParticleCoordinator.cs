namespace BovineLabs.Anchor.Particles
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using BovineLabs.Anchor.Elements;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class UIParticleCoordinator
    {
        public const int DefaultParticleSlotBudget = 16384;
        [NoAutoStaticsCleanup]
        private static readonly ConditionalWeakTable<IPanel, UIParticleCoordinator> Coordinators = new();
        [NoAutoStaticsCleanup]
        private static readonly List<UIParticleCoordinator> Active = new();
        private readonly List<AnchorParticles> elements = new();
        private readonly List<AnchorParticles> notifications = new();
        private readonly IVisualElementScheduledItem scheduled;
        private int lastFrame = -1;
        private int particleSlotBudget = DefaultParticleSlotBudget;

        private UIParticleCoordinator(IPanel panel)
        {
            this.scheduled = panel.visualTree.schedule.Execute(this.Update).Every(1);
            this.scheduled.Pause();
        }

        public int ParticleSlotBudget
        {
            get => this.particleSlotBudget;
            set
            {
                if (value < this.ReservedSlots)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Budget cannot be below resident capacity.");
                }

                this.particleSlotBudget = value;
            }
        }

        public int ReservedSlots { get; private set; }
        public ulong RejectedPlays { get; private set; }
        public int LiveCount
        {
            get
            {
                var live = 0;
                foreach (var element in this.elements)
                {
                    live += element.LiveCount;
                }

                return live;
            }
        }

        // Weak ownership preserves panel configuration across its last detach without retaining a discarded panel.
        public static UIParticleCoordinator Get(IPanel panel)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(nameof(panel));
            }

            return Coordinators.GetValue(panel, static p => new UIParticleCoordinator(p));
        }

        internal static UIParticleCoordinator Register(IPanel panel, AnchorParticles element)
        {
            var coordinator = Get(panel);
            if (!coordinator.elements.Contains(element))
            {
                if (coordinator.elements.Count == 0)
                {
                    Active.Add(coordinator);
                }

                coordinator.elements.Add(element);
            }

            return coordinator;
        }

        internal bool TryReserve(int capacity)
        {
            if (capacity > this.particleSlotBudget - this.ReservedSlots)
            {
                if (this.RejectedPlays != ulong.MaxValue)
                {
                    this.RejectedPlays++;
                }

                return false;
            }

            this.ReservedSlots += capacity;
            return true;
        }

        internal void Release(int capacity) => this.ReservedSlots -= capacity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            foreach (var coordinator in Active.ToArray())
            {
                coordinator.scheduled.Pause();
                foreach (var element in coordinator.elements.ToArray())
                {
                    element.ReleaseVisualGeneration();
                }

                coordinator.notifications.Clear();
            }

            Active.Clear();
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
            if (this.elements.Remove(element) && this.elements.Count == 0)
            {
                this.scheduled.Pause();
                Active.Remove(this);
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
