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
        private readonly List<AnchorParticles> _elements = new();
        private readonly List<AnchorParticles> _notifications = new();
        private readonly IVisualElementScheduledItem _scheduled;
        private int _lastFrame = -1;
        private int _particleSlotBudget = DefaultParticleSlotBudget;

        private UIParticleCoordinator(IPanel panel)
        {
            _scheduled = panel.visualTree.schedule.Execute(Update).Every(1);
            _scheduled.Pause();
        }

        public int ParticleSlotBudget
        {
            get => _particleSlotBudget;
            set
            {
                if (value < ReservedSlots)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Budget cannot be below resident capacity.");
                }

                _particleSlotBudget = value;
            }
        }

        public int ReservedSlots { get; private set; }
        public ulong RejectedPlays { get; private set; }
        public int LiveCount
        {
            get
            {
                var live = 0;
                foreach (var element in _elements)
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
            if (!coordinator._elements.Contains(element))
            {
                if (coordinator._elements.Count == 0)
                {
                    Active.Add(coordinator);
                }

                coordinator._elements.Add(element);
            }

            return coordinator;
        }

        internal bool TryReserve(int capacity)
        {
            if (capacity > _particleSlotBudget - ReservedSlots)
            {
                if (RejectedPlays != ulong.MaxValue)
                {
                    RejectedPlays++;
                }

                return false;
            }

            ReservedSlots += capacity;
            return true;
        }

        internal void Release(int capacity) => ReservedSlots -= capacity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            foreach (var coordinator in Active.ToArray())
            {
                coordinator._scheduled.Pause();
                foreach (var element in coordinator._elements.ToArray())
                {
                    element.ReleaseVisualGeneration();
                }

                coordinator._notifications.Clear();
            }

            Active.Clear();
            Coordinators.Clear();
        }

        internal void Wake()
        {
            if (Application.isPlaying)
            {
                _scheduled.Resume();
            }
        }

        internal void Unregister(AnchorParticles element)
        {
            if (_elements.Remove(element) && _elements.Count == 0)
            {
                _scheduled.Pause();
                Active.Remove(this);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || _lastFrame == Time.frameCount)
            {
                return;
            }

            _lastFrame = Time.frameCount;
            var unscaled = (double)Time.unscaledDeltaTime;
            var scaled = (double)Time.deltaTime;
            foreach (var element in _elements)
            {
                if (!element.ManualClock && element.NeedsUpdate)
                {
                    element.Tick(element.TimeMode == UIParticleTimeMode.Scaled ? scaled : unscaled);
                    _notifications.Add(element);
                }
            }

            // Handlers may detach controls, replace assets, or restart this or another run.
            for (var i = 0; i < _notifications.Count; i++)
            {
                _notifications[i].DispatchCompletion();
            }

            _notifications.Clear();
            foreach (var element in _elements)
            {
                if (!element.ManualClock && element.NeedsUpdate)
                {
                    return;
                }
            }

            _scheduled.Pause();
        }
    }
}
