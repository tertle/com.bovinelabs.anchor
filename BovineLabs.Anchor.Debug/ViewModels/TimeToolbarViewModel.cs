namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Time")]
    public class TimeToolbarViewModel : ObservableObject, IToolbarElement
    {
        private float _timescale;
        private long _unscaledSeconds;
        private long _seconds;

        [CreateProperty]
        public float TimeScale
        {
            get => _timescale;
            set
            {
                value = Mathf.Clamp(value, 0, 100);
                if (SetProperty(ref _timescale, value))
                {
                    Time.timeScale = TimeScale;
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public long UnscaledSeconds
        {
            get => _unscaledSeconds;
            set => SetProperty(ref _unscaledSeconds, value);
        }

        [CreateProperty(ReadOnly = true)]
        public long Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value);
        }

        public VisualElement CreateElement()
        {
            return new TimeToolbarView(this);
        }

        public void Update()
        {
            TimeScale = Time.timeScale;
            UnscaledSeconds = (long)Time.unscaledTimeAsDouble;
            Seconds = (long)Time.timeAsDouble;
        }

        public static float TimescaleToUI(float value)
        {
            return value switch
            {
                <= 2 => value,
                _ => (value + 14) / 8,
            };
        }

        public static float UIToTimeScale(float value)
        {
            return value switch
            {
                <= 0 => 0.1f,
                <= 2 => value,
                _ => (8 * value) - 14,
            };
        }
    }
}
