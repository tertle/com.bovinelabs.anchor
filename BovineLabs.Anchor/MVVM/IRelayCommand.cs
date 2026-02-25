// <copyright file="IRelayCommand.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System.Windows.Input;

    /// <summary>
    /// ICommand extension for notifying can-execute changes.
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        void NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Strongly-typed relay command contract.
    /// </summary>
    /// <typeparam name="T">Command parameter type.</typeparam>
    public interface IRelayCommand<in T> : IRelayCommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        void Execute(T parameter);

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True when execution is allowed.</returns>
        bool CanExecute(T parameter);
    }
}
