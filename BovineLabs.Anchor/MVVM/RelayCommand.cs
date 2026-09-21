namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.ComponentModel;

    public sealed class RelayCommand : IRelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly string[] _observedProperties;

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(execute, canExecute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, INotifyPropertyChanged propertyChangedSource, params string[] observedProperties)
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
            return _canExecute?.Invoke() ?? true;
        }

        public bool CanExecute()
        {
            return CanExecute(null);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            _execute();
        }

        public void Execute()
        {
            Execute(null);
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
    }
}
