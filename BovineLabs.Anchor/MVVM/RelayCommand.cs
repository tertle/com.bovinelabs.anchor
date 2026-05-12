// <copyright file="RelayCommand.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Parameterless relay command implementation.
    /// </summary>
    public sealed class RelayCommand : IRelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        private readonly string[] observedProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">Execution callback.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">Execution callback.</param>
        /// <param name="canExecute">Optional can-execute callback.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(execute, canExecute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">Execution callback.</param>
        /// <param name="canExecute">Optional can-execute callback.</param>
        /// <param name="propertyChangedSource">Optional property source that invalidates can-execute.</param>
        /// <param name="observedProperties">Properties that invalidate can-execute when changed.</param>
        public RelayCommand(Action execute, Func<bool> canExecute, INotifyPropertyChanged propertyChangedSource, params string[] observedProperties)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            this.observedProperties = observedProperties ?? Array.Empty<string>();

            if (propertyChangedSource != null && this.observedProperties.Length != 0)
            {
                propertyChangedSource.PropertyChanged += this.OnPropertyChanged;
            }
        }

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return this.canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// Determines whether this command can execute.
        /// </summary>
        /// <returns>True when execution is allowed.</returns>
        public bool CanExecute()
        {
            return this.CanExecute(null);
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            if (!this.CanExecute(parameter))
            {
                return;
            }

            this.execute();
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        public void Execute()
        {
            this.Execute(null);
        }

        /// <inheritdoc/>
        public void NotifyCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || Array.IndexOf(this.observedProperties, e.PropertyName) >= 0)
            {
                this.NotifyCanExecuteChanged();
            }
        }
    }
}
