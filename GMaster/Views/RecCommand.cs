using System;
using System.ComponentModel;
using System.Windows.Input;

namespace GMaster.Views
{
    public class RecCommand : ICommand
    {
        private readonly CameraViewModel cameraViewModel;

        public RecCommand(CameraViewModel cameraViewModel)
        {
            this.cameraViewModel = cameraViewModel;
            cameraViewModel.PropertyChanged += CameraViewModel_PropertyChanged;
        }

        private void CameraViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
            => cameraViewModel.SelectedCamera != null && cameraViewModel.SelectedCamera.RecState != RecState.Unknown;

        public async void Execute(object parameter)
        {
            var lumix = cameraViewModel.SelectedCamera;
            if (lumix == null) return;

            if (lumix.RecState == RecState.Stopped)
            {
                await lumix.RecStart();
            }
            else if (lumix.RecState == RecState.Started)
            {
                await lumix.RecStop();
            }
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public class CaptureCommand : ICommand
    {
        private readonly CameraViewModel cameraViewModel;

        public CaptureCommand(CameraViewModel cameraViewModel)
        {
            this.cameraViewModel = cameraViewModel;
            cameraViewModel.PropertyChanged += CameraViewModel_PropertyChanged;
        }

        private void CameraViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) => cameraViewModel.SelectedCamera != null;

        public async void Execute(object parameter)
        {
            var lumix = cameraViewModel.SelectedCamera;
            if (lumix == null) return;
            await lumix.Capture();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}