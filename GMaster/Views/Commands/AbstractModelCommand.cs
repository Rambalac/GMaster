using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GMaster.Views
{
    public abstract class AbstractModelCommand<TModel> : ICommand
        where TModel : INotifyPropertyChanged
    {
        protected AbstractModelCommand(TModel model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public event EventHandler CanExecuteChanged;

        protected TModel Model { get; }

        public bool CanExecute(object parameter)
        {
            return InternalCanExecute();
        }

        public async void Execute(object parameter)
        {
            await InternalExecute();
        }

        protected abstract bool InternalCanExecute();

        protected abstract Task InternalExecute();

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