namespace GMaster.Tools
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class AbstractParameterModelCommand<TModel, TParameter> : ICommand
        where TModel : INotifyPropertyChanged
    {
        protected AbstractParameterModelCommand(TModel model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public event EventHandler CanExecuteChanged;

        protected TModel Model { get; }

        public bool CanExecute(object parameter)
        {
            return InternalCanExecute((TParameter)parameter);
        }

        public async void Execute(object parameter)
        {
            await InternalExecute((TParameter)parameter);
        }

        protected abstract bool InternalCanExecute(TParameter parameter);

        protected abstract Task InternalExecute(TParameter parameter);

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}