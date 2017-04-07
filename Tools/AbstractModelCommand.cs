namespace Tools
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class AbstractModelCommand<TModel> : AbstractModelUser<TModel>, ICommand
        where TModel : INotifyPropertyChanged
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => Model != null && InternalCanExecute();

        public async void Execute(object parameter)
        {
            await InternalExecute();
        }

        protected abstract bool InternalCanExecute();

        protected abstract Task InternalExecute();

        protected override void ModelChanged(object eOldValue, object eNewValue)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            if (eOldValue != null)
            {
                ((TModel)eOldValue).PropertyChanged -= AbstractModelCommand_PropertyChanged;
            }

            if (eNewValue != null)
            {
                ((TModel)eNewValue).PropertyChanged += AbstractModelCommand_PropertyChanged;
            }
        }

        private void AbstractModelCommand_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}