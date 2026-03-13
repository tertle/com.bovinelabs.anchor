// <copyright file="RelayCommand{T}.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    /// <summary>
    /// Generic relay command implementation.
    /// </summary>
    /// <typeparam name="T">Command parameter type.</typeparam>
    public sealed class RelayCommand<T> : IRelayCommand<T>
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">Execution callback.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">Execution callback.</param>
        /// <param name="canExecute">Optional can-execute callback.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                return false;
            }

            return this.CanExecute(typed);
        }

        /// <inheritdoc/>
        public bool CanExecute(T parameter)
        {
            return this.canExecute?.Invoke(parameter) ?? true;
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                throw new InvalidOperationException("Invalid parameter type.");
            }

            this.Execute(typed);
        }

        /// <inheritdoc/>
        public void Execute(T parameter)
        {
            if (!this.CanExecute(parameter))
            {
                return;
            }

            this.execute(parameter);
        }

        /// <inheritdoc/>
        public void NotifyCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private static bool TryGetCommandArg(object parameter, out T result)
        {
            if (parameter == null)
            {
                result = default;
                return !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;
            }

            if (parameter is T typed)
            {
                result = typed;
                return true;
            }

            result = default;
            return false;
        }
    }
}
