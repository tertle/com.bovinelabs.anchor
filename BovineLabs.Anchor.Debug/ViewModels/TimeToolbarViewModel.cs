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
    public partial class TimeToolbarViewModel : ObservableObject, IToolbarElement
    {
        [CreateProperty]
        public float TimeScale
        {
            get;
            set
            {
                value = Mathf.Clamp(value, 0, 100);
                if (SetProperty(ref field, value))
                {
                    Time.timeScale = TimeScale;
                }
            }
        }

        [ObservableProperty]
        [CreateProperty(ReadOnly = true)]
        public partial long UnscaledSeconds
        {
            get;
            set;
        }

        [ObservableProperty]
        [CreateProperty(ReadOnly = true)]
        public partial long Seconds
        {
            get;
            set;
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
