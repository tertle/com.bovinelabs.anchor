namespace BovineLabs.Anchor.Elements
{
    using System.Windows.Input;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class Timer : VisualElement
    {
        private static readonly BindingId CommandProperty = nameof(command);
        private static readonly BindingId IntervalProperty = nameof(interval);

        private ICommand _command;
        private long _interval;
        private IVisualElementScheduledItem _scheduledItem;

        /// <summary>
        /// Interval in milliseconds; zero disables scheduling.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public long interval
        {
            get => _interval;
            set
            {
                if (_interval != value)
                {
                    _interval = value;

                    _scheduledItem?.Pause();
                    _scheduledItem = null;

                    if (value > 0)
                    {
                        _scheduledItem = schedule.Execute(_ => command?.Execute(null)).Every(value);
                    }

                    NotifyPropertyChanged(in IntervalProperty);
                }
            }
        }

        [CreateProperty]
        public ICommand command
        {
            get => _command;
            set
            {
                if (_command != value)
                {
                    _command = value;
                    NotifyPropertyChanged(in CommandProperty);
                }
            }
        }
    }
}
