namespace BovineLabs.Anchor.Components
{
    using System;
    using System.Windows.Input;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [VisualElementComponent]
    public partial struct Timer : IVisualElementComponent
    {
        /// <summary>Interval in milliseconds; zero disables scheduling.</summary>
        [CreateProperty]
        [UxmlAttribute("interval")]
        public long Interval;

        [CreateProperty]
        public ICommand Command;

        private TimerSchedule _schedule;

        [OnComponentAdded]
        private static void OnAdded(ref Timer self, VisualElement owner)
        {
            self._schedule = new TimerSchedule(owner);
            self._schedule.Apply(self.Interval);
        }

        [OnComponentChanged]
        private static void OnChanged(ref Timer self, VisualElement owner)
        {
            self._schedule.Apply(self.Interval);
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref Timer self, VisualElement owner)
        {
            self._schedule.Dispose();
        }

        [ReleaseComponentResources]
        private static void Release(ref Timer self)
        {
            self._schedule.Dispose();
        }

        private sealed class TimerSchedule : IDisposable
        {
            private readonly VisualElement _owner;
            private IVisualElementScheduledItem _scheduledItem;
            private long _interval;
            private bool _disposed;

            public TimerSchedule(VisualElement owner)
            {
                _owner = owner;
            }

            public void Apply(long interval)
            {
                if (_interval == interval)
                {
                    return;
                }

                _scheduledItem?.Pause();
                _scheduledItem = null;
                _interval = interval;

                if (interval > 0)
                {
                    _scheduledItem = _owner.schedule.Execute(OnTick).Every(interval);
                }
            }

            public void Dispose()
            {
                _disposed = true;
                _scheduledItem?.Pause();
                _scheduledItem = null;
            }

            private void OnTick()
            {
                // A due callback can outlive removal by an earlier callback in the same scheduler update.
                if (_disposed)
                {
                    return;
                }

                _owner.GetComponent<Timer>().Command?.Execute(null);
            }
        }
    }
}
