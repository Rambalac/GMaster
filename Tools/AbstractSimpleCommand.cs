namespace Tools
{
    using System;
    using System.Windows.Input;

    public abstract class AbstractSimpleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            InternalExecute();
        }

        protected abstract void InternalExecute();

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}