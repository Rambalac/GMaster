using System;

namespace GMaster.Views
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Camera;
    using Commands;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private Lumix selectedCamera;

        public CameraViewModel()
        {
            RecCommand = new RecCommand(this);
            CaptureCommand = new CaptureCommand(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CaptureCommand CaptureCommand { get; }

        public bool IsDisconnected => selectedCamera == null;

        public RecCommand RecCommand { get; }

        public Lumix SelectedCamera
        {
            get
            {
                return selectedCamera;
            }

            set
            {
                if (selectedCamera != null)
                {
                    selectedCamera.Disconnected -= SelectedCamera_Disconnected;
                }

                selectedCamera = value;
                if (selectedCamera != null)
                {
                    selectedCamera.Disconnected += SelectedCamera_Disconnected;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDisconnected));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectedCamera_Disconnected(Lumix lumix, bool stillAvailable)
        {
            if (ReferenceEquals(lumix, SelectedCamera))
            {
                SelectedCamera = null;
            }
        }
    }
}