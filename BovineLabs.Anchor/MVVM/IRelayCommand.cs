namespace BovineLabs.Anchor.MVVM
{
    using System.Windows.Input;

    public interface IRelayCommand : ICommand
    {
        void NotifyCanExecuteChanged();
    }

    public interface IRelayCommand<in T> : IRelayCommand
    {
        void Execute(T parameter);

        bool CanExecute(T parameter);
    }
}
