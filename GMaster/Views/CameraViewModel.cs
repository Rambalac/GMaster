using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GMaster.Annotations;
using GMaster.Camera;
using GMaster.Views.Commands;

namespace GMaster.Views
{
    public class CameraViewModel : INotifyPropertyChanged
    {
        private CameraMenuItem currentIso;
        private CameraMenuItem currentShutter;
        private CameraMenuItem currentAperture;
        private ConnectedCamera selectedCamera;

        public CameraViewModel()
        {
            RecCommand = new RecCommand(this);
            CaptureCommand = new CaptureCommand(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CaptureCommand CaptureCommand { get; }

        public CameraMenuItem CurrentIso
        {
            get
            {
                return currentIso;
            }

            set
            {
                AsyncMenuItemSetter(currentIso, value, v =>
                {
                    currentIso = v;
                    OnPropertyChanged(nameof(CurrentIso));
                });
            }
        }

        public CameraMenuItem CurrentShutter
        {
            get
            {
                return currentShutter;
            }

            set
            {
                AsyncMenuItemSetter(currentShutter, value, v =>
                {
                    currentShutter = v;
                    OnPropertyChanged(nameof(CurrentShutter));
                });
            }
        }

        public CameraMenuItem CurrentAperture
        {
            get
            {
                return currentAperture;
            }

            set
            {
                AsyncMenuItemSetter(currentAperture, value, v =>
                {
                    currentAperture = v;
                    OnPropertyChanged(nameof(CurrentAperture));
                });
            }
        }

        public bool IsConnected => selectedCamera != null;

        public bool IsDisconnected => selectedCamera == null;

        public Stream LiveViewFrame => SelectedCamera?.Camera?.LiveViewFrame;

        public RecCommand RecCommand { get; }

        public ConnectedCamera SelectedCamera
        {
            get
            {
                return selectedCamera;
            }

            set
            {
                if (selectedCamera != null)
                {
                    selectedCamera.Camera.Disconnected -= SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged -= Camera_PropertyChanged;
                    selectedCamera.Camera.OfframeProcessor.PropertyChanged -= OfframeProcessor_PropertyChanged;
                }

                selectedCamera = value;
                if (selectedCamera != null)
                {
                    selectedCamera.Camera.Disconnected += SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged += Camera_PropertyChanged;
                    selectedCamera.Camera.OfframeProcessor.PropertyChanged += OfframeProcessor_PropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDisconnected));
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(CurrentIso));
                OnPropertyChanged(nameof(LiveViewFrame));
            }
        }

        private void OfframeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(selectedCamera.Camera.OfframeProcessor.Shutter):
                    currentShutter = selectedCamera.Camera.GetCurrentShutter() ?? currentShutter;
                    OnPropertyChanged(nameof(CurrentShutter));
                    break;
                case nameof(selectedCamera.Camera.OfframeProcessor.Aperture):
                    currentAperture = selectedCamera.Camera.GetCurrentAperture() ?? currentAperture;
                    OnPropertyChanged(nameof(CurrentAperture));
                    break;
                case nameof(selectedCamera.Camera.OfframeProcessor.Iso):
                    currentIso = selectedCamera.Camera.GetCurrentIso() ?? currentIso;
                    OnPropertyChanged(nameof(CurrentIso));
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void AsyncSetter<TValue>(TValue oldvalue, TValue newvalue, Func<TValue, Task> action, Action<TValue> result)
        {
            Task.Run(async () =>
            {
                try
                {
                    await action(newvalue);
                    await App.RunAsync(() => result(newvalue));
                }
                catch (Exception ex)
                {
                    await App.RunAsync(() => result(oldvalue));
                }
            });
        }

        private void AsyncMenuItemSetter(CameraMenuItem old, CameraMenuItem value, Action<CameraMenuItem> onResult)
        {
            AsyncSetter(
                old,
                value,
                async v => await selectedCamera.Camera.SendMenuItem(v),
                onResult);
        }

        private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(selectedCamera.Camera.LiveViewFrame):
                    OnPropertyChanged(nameof(LiveViewFrame));
                    break;
            }
        }

        private void SelectedCamera_Disconnected(Lumix lumix, bool stillAvailable)
        {
            if (ReferenceEquals(lumix, SelectedCamera.Camera))
            {
                SelectedCamera = null;
            }
        }
    }
}