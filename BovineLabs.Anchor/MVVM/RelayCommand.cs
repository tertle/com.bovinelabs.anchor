// <copyright file="RelayCommand.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    /// <summary>
    /// Parameterless relay command implementation.
    /// </summary>
    public sealed class RelayCommand : IRelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

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
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
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
    }
}
