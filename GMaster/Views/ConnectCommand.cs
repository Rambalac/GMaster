using System;
using System.ComponentModel;
using System.Windows.Input;

namespace GMaster.Views
{
    public class ConnectCommand : ICommand
    {
        private readonly CameraViewModel model;

        public ConnectCommand(CameraViewModel cameraViewModel)
        {
            this.model = cameraViewModel;
            model.PropertyChanged += Model_PropertyChanged;
        }

        public bool CanExecute(object parameter) => model.SelectedDevice != null && model.SelectedCamera == null;

        public async void Execute(object parameter)
        {
            var lumix = new Lumix(model.SelectedDevice);
            await lumix.Connect();
            model.GlobalModel.AddConnectedDevice(lumix);
            model.SelectedCamera = lumix;
        }

        public event EventHandler CanExecuteChanged;

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(model.SelectedDevice) || e.PropertyName == nameof(model.SelectedCamera))
                OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}