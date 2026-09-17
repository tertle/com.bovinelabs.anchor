namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.ComponentModel;

    public sealed class RelayCommand<T> : IRelayCommand<T>
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;
        private readonly string[] observedProperties;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
            : this(execute, canExecute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute, INotifyPropertyChanged propertyChangedSource, params string[] observedProperties)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            this.observedProperties = observedProperties ?? Array.Empty<string>();

            if (propertyChangedSource != null && this.observedProperties.Length != 0)
            {
                propertyChangedSource.PropertyChanged += this.OnPropertyChanged;
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                return false;
            }

            return this.CanExecute(typed);
        }

        public bool CanExecute(T parameter)
        {
            return this.canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                throw new InvalidOperationException("Invalid parameter type.");
            }

            this.Execute(typed);
        }

        public void Execute(T parameter)
        {
            if (!this.CanExecute(parameter))
            {
                return;
            }

            this.execute(parameter);
        }

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
