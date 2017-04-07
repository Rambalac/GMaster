namespace GMaster.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Camera;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private ICameraMenuItem currentAperture;
        private ICameraMenuItem currentIso;
        private ICameraMenuItem currentShutter;
        private ConnectedCamera selectedCamera;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanChangeAperture => SelectedCamera?.Camera?.CanChangeAperture ?? true;

        public bool CanChangeShutter => SelectedCamera?.Camera?.CanChangeShutter ?? true;

        public object CanManualFocus => SelectedCamera?.Camera?.CanManualFocus ?? false;

        public ICameraMenuItem CurrentAperture
        {
            get => currentAperture;

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

        public ICollection<CameraMenuItem256> CurrentApertures => SelectedCamera?.Camera?.CurrentApertures;

        public ICameraMenuItem CurrentIso
        {
            get => currentIso;

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
            get => currentShutter;

            set
            {
                AsyncMenuItemSetter(currentShutter, value, v =>
                {
                    currentShutter = v;
                    OnPropertyChanged(nameof(CurrentShutter));
                });
            }
        }

        public FocusPoint FocusPoint { get; private set; }

        public bool HasPowerZoom => SelectedCamera?.Camera?.LensInfo?.HasPowerZoom ?? false;

        public bool IsConnected => selectedCamera != null;

        public TitledList<CameraMenuItemText> IsoValues => SelectedCamera?.Camera?.MenuSet?.IsoValues;

        public RecState? RecState => SelectedCamera?.Camera?.RecState;

        public ConnectedCamera SelectedCamera
        {
            get => selectedCamera;

            set
            {
                if (selectedCamera != null)
                {
                    selectedCamera.Camera.Disconnected -= SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged -= Camera_PropertyChanged;
                    selectedCamera.Camera.OffFrameProcessor.PropertyChanged -= OfframeProcessor_PropertyChanged;
                }

                selectedCamera = value;
                if (selectedCamera != null)
                {
                    selectedCamera.Camera.Disconnected += SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged += Camera_PropertyChanged;
                    selectedCamera.Camera.OffFrameProcessor.PropertyChanged += OfframeProcessor_PropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanChangeAperture));
                OnPropertyChanged(nameof(CanChangeShutter));
                OnPropertyChanged(nameof(HasPowerZoom));
                OnPropertyChanged(nameof(CanManualFocus));
                OnPropertyChanged(nameof(ShutterSpeeds));
                OnPropertyChanged(nameof(CurrentApertures));
                OnPropertyChanged(nameof(IsoValues));
                OnPropertyChanged(nameof(CurrentAperture));
                OnPropertyChanged(nameof(CurrentShutter));
                OnPropertyChanged(nameof(CurrentIso));
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public TitledList<CameraMenuItemText> ShutterSpeeds => SelectedCamera?.Camera?.MenuSet?.ShutterSpeeds;

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
                case nameof(Lumix.CanManualFocus):
                    OnPropertyChanged(nameof(CanManualFocus));
                    break;

                case nameof(Lumix.CurrentApertures):
                    OnPropertyChanged(nameof(CurrentApertures));
                    break;

                case nameof(Lumix.CanChangeShutter):
                    OnPropertyChanged(nameof(CanChangeShutter));
                    break;

                case nameof(Lumix.CanChangeAperture):
                    OnPropertyChanged(nameof(CanChangeAperture));
                    break;

                case nameof(Lumix.MenuSet):
                    OnPropertyChanged(nameof(ShutterSpeeds));
                    OnPropertyChanged(nameof(IsoValues));
                    break;

                case nameof(Lumix.RecState):
                    OnPropertyChanged(nameof(RecState));
                    break;

                case nameof(Lumix.LensInfo):
                    OnPropertyChanged(nameof(HasPowerZoom));
                    break;
            }
        }

        private void OfframeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var camera = selectedCamera.Camera;
            switch (e.PropertyName)
            {
                case nameof(OffFrameProcessor.Shutter):
                    currentShutter = camera.CurrentShutter ?? currentShutter;
                    OnPropertyChanged(nameof(CurrentShutter));
                    break;

                case nameof(OffFrameProcessor.Aperture):
                    currentAperture = camera.CurrentAperture ?? currentAperture;
                    OnPropertyChanged(nameof(CurrentAperture));
                    break;

                case nameof(OffFrameProcessor.Iso):
                    currentIso = camera.CurrentIso ?? currentIso;
                    OnPropertyChanged(nameof(CurrentIso));
                    break;

                case nameof(OffFrameProcessor.FocusPoint):
                    FocusPoint = camera.OffFrameProcessor.FocusPoint;
                    OnPropertyChanged(nameof(FocusPoint));
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