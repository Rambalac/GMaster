using System;
using System.Windows.Input;

namespace LumixMaster
{
    public class ConnectCommand : ICommand
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

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(model.SelectedDevice) || e.PropertyName == nameof(model.SelectedCamera)) OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) => model.SelectedDevice != null && model.SelectedCamera == null;

        public async void Execute(object parameter)
        {
            var lumix = new Lumix(model.SelectedDevice);
            await lumix.Connect();
            model.AddConnectedDevice(lumix);
            model.SelectedCamera = lumix;
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}