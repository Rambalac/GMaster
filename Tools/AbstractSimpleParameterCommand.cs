namespace Tools
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class AbstractSimpleParameterCommand<TParameter> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter)
        {
            await InternalExecute((TParameter)parameter);
        }

        public abstract Task InternalExecute(TParameter parameter);

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}