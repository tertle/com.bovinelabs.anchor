namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.ComponentModel;

    public sealed class RelayCommand<T> : IRelayCommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        private readonly string[] _observedProperties;

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
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _observedProperties = observedProperties ?? Array.Empty<string>();

            if (propertyChangedSource != null && _observedProperties.Length != 0)
            {
                propertyChangedSource.PropertyChanged += OnPropertyChanged;
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                return false;
            }

            return CanExecute(typed);
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (!TryGetCommandArg(parameter, out var typed))
            {
                throw new InvalidOperationException("Invalid parameter type.");
            }

            Execute(typed);
        }

        public void Execute(T parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            _execute(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || Array.IndexOf(_observedProperties, e.PropertyName) >= 0)
            {
                NotifyCanExecuteChanged();
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
