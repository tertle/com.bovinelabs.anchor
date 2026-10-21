namespace BovineLabs.Anchor.Components
{
    using System;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    [VisualElementComponent]
    public partial struct Pulse
    {
        [CreateProperty]
        [UxmlAttribute("enabled")]
        public bool Enabled;

        /// <summary>Duration of a complete pulse in milliseconds; zero disables animation.</summary>
        [CreateProperty]
        [UxmlAttribute("interval")]
        [StyleProperty("--bl-anchor-pulse-interval")]
        public float Interval;

        [CreateProperty]
        [UxmlAttribute("minimum-opacity")]
        [StyleProperty("--bl-anchor-pulse-minimum-opacity")]
        public float MinimumOpacity;

        [CreateProperty]
        [UxmlAttribute("maximum-opacity")]
        [StyleProperty("--bl-anchor-pulse-maximum-opacity")]
        public float MaximumOpacity;

        private PulseAnimation _animation;

        [UxmlCreateInstanceMethod]
        public static Pulse Create()
        {
            return new Pulse
            {
                Interval = 1400f,
                MinimumOpacity = 0.35f,
                MaximumOpacity = 1f,
            };
        }

        [OnComponentAdded]
        private static void OnAdded(ref Pulse self, VisualElement owner)
        {
            self._animation = new PulseAnimation(owner);
            self._animation.Apply(ref self);
        }

        [OnComponentChanged]
        private static void OnChanged(ref Pulse self, VisualElement owner)
        {
            self._animation.Apply(ref self);
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref Pulse self, VisualElement owner)
        {
            self._animation.Stop();
        }

        [ReleaseComponentResources]
        private static void Release(ref Pulse self)
        {
            self._animation.Dispose();
        }

        private sealed class PulseAnimation : IDisposable
        {
            private readonly VisualElement _owner;
            private IVisualElementScheduledItem _scheduledItem;
            private StyleFloat _originalOpacity;
            private bool _running;
            private bool _hasStarted;
            private long _startedAt;

            public PulseAnimation(VisualElement owner)
            {
                _owner = owner;
            }

            public void Apply(ref Pulse pulse)
            {
                if (!pulse.Enabled || pulse.Interval <= 0f)
                {
                    Stop();
                    return;
                }

                if (_running)
                {
                    return;
                }

                _originalOpacity = _owner.style.opacity;
                _running = true;
                _hasStarted = false;
                _scheduledItem ??= _owner.schedule.Execute(OnUpdate).Every(16);
                _scheduledItem.ExecuteLater(0);
            }

            public void Stop()
            {
                if (!_running)
                {
                    return;
                }

                _scheduledItem.Pause();
                _owner.style.opacity = _originalOpacity;
                _running = false;
            }

            public void Dispose()
            {
                _scheduledItem?.Pause();
                _scheduledItem = null;
            }

            private void OnUpdate(TimerState timer)
            {
                ref var pulse = ref _owner.GetComponent<Pulse>();
                if (!pulse.Enabled || pulse.Interval <= 0f)
                {
                    Stop();
                    return;
                }

                if (!_hasStarted)
                {
                    _startedAt = timer.now;
                    _hasStarted = true;
                }

                var phase = ((timer.now - _startedAt) % pulse.Interval) / pulse.Interval;
                var blend = (1f - Mathf.Cos(phase * 2f * Mathf.PI)) * 0.5f;
                _owner.style.opacity = Mathf.Lerp(Mathf.Clamp01(pulse.MaximumOpacity), Mathf.Clamp01(pulse.MinimumOpacity), blend);
            }
        }
    }
}
