// <copyright file="Timer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Windows.Input;
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Visual element that repeatedly invokes a command on a schedule.
    /// </summary>
    [UxmlElement]
    public partial class Timer : VisualElement
    {
        private static readonly BindingId CommandProperty = nameof(command);
        private static readonly BindingId IntervalProperty = nameof(interval);

        private ICommand m_command;
        private long m_interval;
        private IVisualElementScheduledItem m_scheduledItem;

        /// <summary>
        /// Gets or sets the delay between command executions, in milliseconds. A value of 0 disables scheduling.
        /// </summary>
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
                        this.m_scheduledItem = this.schedule.Execute(_ => this.command?.Execute(null)).Every(value);
                    }

                    this.NotifyPropertyChanged(in IntervalProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the command invoked on each interval tick.
        /// </summary>
        [CreateProperty]
        public ICommand command
        {
            get => this.m_command;
            set
            {
                if (this.m_command != value)
                {
                    this.m_command = value;
                    this.NotifyPropertyChanged(in CommandProperty);
                }
            }
        }
    }
}
