namespace GMaster.Views
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Camera;
    using Commands;
    using Windows.ApplicationModel;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private ICameraMenuItem currentAperture;
        private ICameraMenuItem currentIso;
        private ICameraMenuItem currentShutter;
        private ConnectedCamera selectedCamera;

        public CameraViewModel()
        {
            RecCommand = new RecCommand(this);
            CaptureCommand = new CaptureCommand(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CaptureCommand CaptureCommand { get; }

        public ICameraMenuItem CurrentAperture
        {
            get
            {
                return currentAperture;
            }

            set
            {
                var newAper = value ?? currentAperture;

                AsyncMenuItemSetter(currentAperture, newAper, v =>
                {
                    currentAperture = v;
                    OnPropertyChanged(nameof(CurrentAperture));
                });
            }
        }

        public ICameraMenuItem CurrentIso
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

        public ICameraMenuItem CurrentShutter
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

        public bool IsConnected => DesignMode.DesignModeEnabled || selectedCamera != null;

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
                    Debug.WriteLine(ex);
                    await App.RunAsync(() => result(oldvalue));
                }
            });
        }

        private void AsyncMenuItemSetter(ICameraMenuItem old, ICameraMenuItem value, Action<ICameraMenuItem> onResult)
        {
            AsyncSetter(
                old,
                value,
                async v =>
                {
                    if (selectedCamera != null)
                    {
                        await selectedCamera.Camera.SendMenuItem(v);
                    }
                },
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

        private void OfframeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(selectedCamera.Camera.OfframeProcessor.Shutter):
                    currentShutter = selectedCamera.Camera.CurrentShutter ?? currentShutter;
                    OnPropertyChanged(nameof(CurrentShutter));
                    break;
                case nameof(selectedCamera.Camera.OfframeProcessor.Aperture):
                    currentAperture = selectedCamera.Camera.CurrentAperture ?? currentAperture;
                    OnPropertyChanged(nameof(CurrentAperture));
                    break;
                case nameof(selectedCamera.Camera.OfframeProcessor.Iso):
                    currentIso = selectedCamera.Camera.CurrentIso ?? currentIso;
                    OnPropertyChanged(nameof(CurrentIso));
                    break;
                case nameof(selectedCamera.Camera.OfframeProcessor.FocusPoint):
                    FocusPoint = selectedCamera.Camera.OfframeProcessor.FocusPoint;
                    OnPropertyChanged(nameof(FocusPoint));
                    break;
            }
        }

        public FocusPoint FocusPoint { get; private set; }

        private void SelectedCamera_Disconnected(Lumix lumix, bool stillAvailable)
        {
            if (ReferenceEquals(lumix, SelectedCamera.Camera))
            {
                SelectedCamera = null;
            }
        }
    }
}