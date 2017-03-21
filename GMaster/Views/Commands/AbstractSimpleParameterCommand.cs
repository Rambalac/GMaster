namespace GMaster.Views.Commands
{
    using System;
    using System.Windows.Input;

    public abstract class AbstractSimpleParameterCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public abstract void Execute(object parameter);
    }
}