namespace GMaster.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Camera;
    using GMaster.Logger;
    using Tools;
    using Windows.UI.Core;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private ICameraMenuItem currentAperture;
        private ICameraMenuItem currentIso;
        private ICameraMenuItem currentShutter;
        private ConnectedCamera selectedCamera;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanCapture => SelectedCamera?.Camera?.CanCapture ?? false;

        public bool CanChangeAperture => SelectedCamera?.Camera?.CanChangeAperture ?? true;

        public bool CanChangeShutter => SelectedCamera?.Camera?.CanChangeShutter ?? true;

        public object CanManualFocus => SelectedCamera?.Camera?.CanManualFocus ?? false;

        public int CurentZoom => SelectedCamera?.Camera?.OffFrameProcessor?.Zoom ?? 0;

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

        public string CurrentApertureText => SelectedCamera?.Camera?.OffFrameProcessor?.Aperture.Text ?? string.Empty;

        public int CurrentFocus => SelectedCamera?.Camera?.CurrentFocus ?? 0;

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

        public string CurrentShutterText => SelectedCamera?.Camera?.OffFrameProcessor?.Shutter.Text ?? string.Empty;

        public CoreDispatcher Dispatcher { get; set; }

        public FocusPoint FocusPoint => SelectedCamera?.Camera?.OffFrameProcessor?.FocusPoint;

        public bool HasPowerZoom => SelectedCamera?.Camera?.LensInfo?.HasPowerZoom ?? false;

        public bool IsConnected => selectedCamera != null;

        public TitledList<CameraMenuItemText> IsoValues => SelectedCamera?.Camera?.MenuSet?.IsoValues;

        public int MaximumFocus => SelectedCamera?.Camera?.MaximumFocus ?? 0;

        public int MaxZoom => SelectedCamera?.Camera?.LensInfo?.MaxZoom ?? 0;

        public int MinZoom => SelectedCamera?.Camera?.LensInfo?.MinZoom ?? 0;

        public RecState? RecState => SelectedCamera?.Camera?.RecState;

        public ConnectedCamera SelectedCamera
        {
            get => selectedCamera;

            set
            {
                if (selectedCamera?.Camera != null)
                {
                    selectedCamera.Camera.Disconnected -= SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged -= Camera_PropertyChanged;
                    if (selectedCamera.Camera.OffFrameProcessor != null)
                    {
                        selectedCamera.Camera.OffFrameProcessor.PropertyChanged -= OfframeProcessor_PropertyChanged;
                    }
                }

                if (value?.Camera?.OffFrameProcessor != null)
                {
                    selectedCamera = value;
                    selectedCamera.Camera.Disconnected += SelectedCamera_Disconnected;
                    selectedCamera.Camera.PropertyChanged += Camera_PropertyChanged;
                    selectedCamera.Camera.OffFrameProcessor.PropertyChanged += OfframeProcessor_PropertyChanged;

                    SetTime = DateTime.UtcNow;
                }
                else
                {
                    selectedCamera = null;
                    SetTime = DateTime.MinValue;
                }

                var task = RunAsync(() =>
                {
                    try
                    {
                        if (selectedCamera != null)
                        {
                            currentAperture = selectedCamera.Camera.CurrentAperture;
                            currentShutter = selectedCamera.Camera.CurrentShutter;
                            currentIso = selectedCamera.Camera.CurrentIso;
                        }

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(IsConnected));

                        OnPropertyChanged(nameof(CanChangeAperture));
                        OnPropertyChanged(nameof(CanChangeShutter));
                        OnPropertyChanged(nameof(CanManualFocus));

                        OnPropertyChanged(nameof(ShutterSpeeds));
                        OnPropertyChanged(nameof(CurrentApertures));
                        OnPropertyChanged(nameof(IsoValues));

                        OnPropertyChanged(nameof(CurrentAperture));
                        OnPropertyChanged(nameof(CurrentShutter));
                        OnPropertyChanged(nameof(CurrentIso));

                        OnPropertyChanged(nameof(CanCapture));
                        OnPropertyChanged(nameof(HasPowerZoom));
                        OnPropertyChanged(nameof(MaxZoom));
                        OnPropertyChanged(nameof(MinZoom));
                        OnPropertyChanged(nameof(CurentZoom));

                        OnPropertyChanged(nameof(FocusPoint));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
            }
        }

        public DateTime SetTime { get; private set; } = DateTime.MinValue;

        public TitledList<CameraMenuItemText> ShutterSpeeds => SelectedCamera?.Camera?.MenuSet?.ShutterSpeeds;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AsyncMenuItemSetter(ICameraMenuItem old, ICameraMenuItem value, Action<ICameraMenuItem> onResult)
        {
            if (value == null)
            {
                return;
            }

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

        private void AsyncSetter<TValue>(TValue oldvalue, TValue newvalue, Func<TValue, Task> action, Action<TValue> result)
        {
            Task.Run(async () =>
            {
                try
                {
                    await action(newvalue);
                    await RunAsync(() => result(newvalue));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "AsyncSetter");
                    await RunAsync(() => result(oldvalue));
                }
            });
        }

        private async void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await RunAsync(() =>
            {
                try
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

                        case nameof(Lumix.CanCapture):
                            OnPropertyChanged(nameof(CanCapture));
                            break;

                        case nameof(Lumix.MaximumFocus):
                            OnPropertyChanged(nameof(MaximumFocus));
                            break;

                        case nameof(Lumix.CurrentFocus):
                            OnPropertyChanged(nameof(CurrentFocus));
                            break;

                        case nameof(Lumix.LensInfo):
                            OnPropertyChanged(nameof(HasPowerZoom));
                            OnPropertyChanged(nameof(MaxZoom));
                            OnPropertyChanged(nameof(MinZoom));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }

        private async void OfframeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await RunAsync(() =>
            {
                try
                {
                    var camera = selectedCamera.Camera;
                    switch (e.PropertyName)
                    {
                        case nameof(OffFrameProcessor.Shutter):
                            currentShutter = camera.CurrentShutter ?? currentShutter;
                            OnPropertyChanged(nameof(CurrentShutter));
                            OnPropertyChanged(nameof(CurrentShutterText));
                            break;

                        case nameof(OffFrameProcessor.Aperture):
                            currentAperture = camera.CurrentAperture ?? currentAperture;
                            OnPropertyChanged(nameof(CurrentAperture));
                            OnPropertyChanged(nameof(CurrentApertureText));
                            break;

                        case nameof(OffFrameProcessor.Iso):
                            currentIso = camera.CurrentIso ?? currentIso;
                            OnPropertyChanged(nameof(CurrentIso));
                            break;

                        case nameof(OffFrameProcessor.Zoom):
                            OnPropertyChanged(nameof(CurentZoom));
                            break;

                        case nameof(OffFrameProcessor.FocusPoint):
                            OnPropertyChanged(nameof(FocusPoint));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }

        private async Task RunAsync(Action act)
        {
            if (Dispatcher != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => act());
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