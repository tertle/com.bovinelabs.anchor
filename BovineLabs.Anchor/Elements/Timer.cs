// <copyright file="TimerElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Windows.Input;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class Timer : VisualElement
    {
        private static readonly BindingId CommandTimerProperty = nameof(commandTimer);
        private static readonly BindingId IntervalProperty = nameof(interval);

        private ICommand m_command;
        private long m_interval;
        private IVisualElementScheduledItem m_scheduledItem;

        [CreateProperty]
        [UxmlAttribute]
        public long interval
        {
            get => this.m_interval;
            set
            {
                if (this.m_interval != value)
                {
                    this.m_interval = value;

                    this.m_scheduledItem?.Pause();
                    this.m_scheduledItem = null;

                    if (value > 0)
                    {
                        this.m_scheduledItem = this.schedule.Execute(_ => this.commandTimer?.Execute(null)).Every(value);
                    }

                    this.NotifyPropertyChanged(in IntervalProperty);
                }
            }
        }

        [CreateProperty]
        public ICommand commandTimer
        {
            get => this.m_command;
            set
            {
                if (this.m_command != value)
                {
                    this.m_command = value;
                    this.NotifyPropertyChanged(in CommandTimerProperty);
                }
            }
        }
    }
}
