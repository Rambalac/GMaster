using System;
using System.ComponentModel;
using System.Windows.Input;

namespace LumixMaster
{
    public class DisconnectCommand : ICommand
    {
        private MainPageModel model;
        public MainPageModel Model
        {
            set
            {
                model = value;
                model.PropertyChanged += Model_PropertyChanged;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(model.SelectedCamera)) OnCanExecuteChanged();
        }


        public bool CanExecute(object parameter) => model.SelectedCamera != null;

        public async void Execute(object parameter)
        {
            await model.SelectedCamera.Disconnect();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}