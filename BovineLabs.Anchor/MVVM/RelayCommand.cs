namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.ComponentModel;

    public sealed class RelayCommand : IRelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        private readonly string[] observedProperties;

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
            return this.canExecute?.Invoke() ?? true;
        }

        public bool CanExecute()
        {
            return this.CanExecute(null);
        }

        public void Execute(object parameter)
        {
            if (!this.CanExecute(parameter))
            {
                return;
            }

            this.execute();
        }

        public void Execute()
        {
            this.Execute(null);
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
    }
}
