namespace GMaster.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Core.Tools;
    using Windows.UI.Core;
    using CameraApi.Core;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private int lastFocusAreasCount;
        private ICameraState cameraState;
        private ConnectedCamera selectedCamera;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollection<string> Apertures
        {
            get
            {
                var openedAperture = cameraState.OpenedAperture;
                return new[] { openedAperture }
                    .Concat(cameraState.Apertures.
                            Where(a => a.IntValue <= cameraState.LensInfo.ClosedAperture &&
                                    a.IntValue > openedAperture.IntValue))
                    .Select(a => a.Text).ToList();
            }
        }

        public AutoFocusMode AutoFocusMode => cameraState?.AutoFocusMode ?? AutoFocusMode.Unknown;

        public bool BatteryCritical => Math.Abs(BatteryLevel) < 0.01;

        public bool GripBatteryCritical => Math.Abs(GripBatteryLevel) < 0.01;

        public bool GripBatteryPresent => GripBatteryLevel >= 0;

        public float BatteryLevel => GetBatteryLevel(cameraState?.State?.Battery);

        public float GripBatteryLevel => GetBatteryLevel(cameraState?.State?.GripBattery);

        public CameraMode CameraMode => cameraState?.CameraMode ?? CameraMode.Unknown;

        public bool CanCapture => cameraState?.CanCapture ?? false;

        public bool CanChangeAperture => cameraState?.CanChangeAperture ?? true;

        public bool CanChangeShutter => cameraState?.CanChangeShutter ?? true;

        public bool CanManualFocus => cameraState?.CanManualFocus ?? false;

        public bool CanManualFocusAf => (cameraState?.FocusMode ?? FocusMode.Unknown) == FocusMode.MF && (selectedCamera?.Camera.Profile.ManualFocusAF ?? false);

        public bool CanPowerZoom => cameraState?.LensInfo?.HasPowerZoom ?? false;

        public bool CanReleaseTouchAf => (AutoFocusMode.ToValue<AutoFocusModeFlags>().HasFlag(AutoFocusModeFlags.TouchAFRelease)
                                            && FocusAreas != null && FocusAreas.Boxes.Count > 0)
                                         || cameraState?.CameraMode == CameraMode.MFAssist
                                         || (FocusAreas?.Boxes.Any(b => b.Props.Type == FocusAreaType.MfAssistPinP || b.Props.Type == FocusAreaType.MfAssistFullscreen) ?? false);

        public bool CanResetTouchAf => (cameraState?.FocusMode ?? FocusMode.Unknown) == FocusMode.MF ||
                                       (FocusAreas?.Boxes.Any(b => b.Props.Type == FocusAreaType.OneAreaSelected || b.Props.Type == FocusAreaType.FaceOther) ?? false);

        public int CurentZoom => cameraState?.Zoom ?? 0;

        public string CurrentAperture
        {
            get => cameraState?.Aperture.Text;

            set
            {
                AsyncMenuItemSetter(cameraState.MenuSet.Apertures.SingleOrDefault(a => a.Text == value) ?? cameraState.OpenedAperture);
            }
        }

        public int CurrentFocus => cameraState?.CurrentFocus ?? 0;

        public string CurrentIso
        {
            get
            {
                if (cameraState?.Iso.Text == null)
                {
                    return null;
                }

                return cameraState.MenuSet.IsoValues.FirstOrDefault(i => i.Text.EndsWith(cameraState.Iso.Text, StringComparison.OrdinalIgnoreCase)).Text;
            }

            set
            {
                AsyncMenuItemSetter(cameraState?.MenuSet?.IsoValues.SingleOrDefault(a => a.Text == value));
            }
        }

        public string CurrentShutter
        {
            get => cameraState?.Shutter.Text;

            set
            {
                Debug.WriteLine("Shutter set to: " + value, "LumixState");

                AsyncMenuItemSetter(cameraState?.MenuSet?.ShutterSpeeds.SingleOrDefault(a => a.Text == value));
            }
        }

        public CoreDispatcher Dispatcher { get; set; }

        public FocusAreas FocusAreas
        {
            get
            {
                var newval = cameraState?.FocusAreas;
                if (lastFocusAreasCount != (newval?.Boxes.Count ?? 0))
                {
                    lastFocusAreasCount = newval?.Boxes.Count ?? 0;
                    OnPropertyChanged(nameof(CanReleaseTouchAf));
                }

                return newval;
            }
        }

        public FocusMode FocusMode => cameraState?.FocusMode ?? FocusMode.Unknown;

        public bool IsConnected => selectedCamera != null;

        public bool IsConnectionActive => !(cameraState?.IsBusy ?? true);

        public ICollection<string> IsoValues
        {
            get
            {
                return cameraState?.MenuSet?.IsoValues
                    .Where(i => cameraState.CurMenu.Enabled.ContainsKey(i.Id)).Select(i => i.Text).ToList()
                    ?? new List<string>();
            }
        }

        public int MaximumFocus => cameraState?.MaximumFocus ?? 0;

        public int MaxZoom => cameraState?.LensInfo?.MaxZoom ?? 0;

        public bool MemoryCardAccess => cameraState?.State.SdAccess == OnOff.On;

        public bool MemoryCard2Access => cameraState?.State.Sd2Access == OnOff.On;

        public bool MemoryCardError => cameraState != null && (cameraState.State.SdMemory == SdMemorySet.Unset
                                                              || cameraState.State.SdCardStatus != SdCardStatus.WriteEnable);

        public bool MemoryCard2Error => cameraState != null && (cameraState.State.Sd2Memory == SdMemorySet.Unset
                                                              || cameraState.State.Sd2CardStatus != SdCardStatus.WriteEnable);

        public bool MemoryCardPresent => (cameraState?.State?.SdMemory ?? SdMemorySet.Unset) == SdMemorySet.Set;

        public bool MemoryCard2Present => (cameraState?.State?.Sd2Memory ?? SdMemorySet.Unset) == SdMemorySet.Set;

        public string MemoryCardInfo
        {
            get
            {
                if (cameraState?.State == null)
                {
                    return string.Empty;
                }

                if (cameraState.State.SdMemory == SdMemorySet.Unset
                    && cameraState.State.Sd2Memory == SdMemorySet.Unset)
                {
                    return "Not inserted";
                }

                if (cameraState.State.SdCardStatus != SdCardStatus.WriteEnable
                    && cameraState.State.Sd2CardStatus != SdCardStatus.WriteEnable)
                {
                    return "Read Only";
                }

                if (cameraState.State.RemainDisplayType == RemainDisplayType.Time)
                {
                    return TimeSpan.FromSeconds(cameraState.State.VideoRemainCapacity).ToString("hh'h:'mm'm:'ss's'").TrimStart('0', 'h', 'm', ':');
                }

                return cameraState.State.RemainCapacity.ToString();
            }
        }

        public int MinZoom => cameraState?.LensInfo?.MinZoom ?? 0;

        public RecState RecState => cameraState?.RecState ?? RecState.Stopped;

        public ConnectedCamera SelectedCamera
        {
            get => selectedCamera;

            set
            {
                if (ReferenceEquals(selectedCamera, value))
                {
                    return;
                }

                if (selectedCamera != null)
                {
                    cameraState.PropertyChanged -= Camera_PropertyChanged;
                    selectedCamera.Removed -= SelectedCamera_Removed;
                    selectedCamera.Camera.ProfileUpdated -= Camera_ProfileUpdated;
                }

                selectedCamera = value;

                if (selectedCamera != null)
                {
                    selectedCamera.Removed += SelectedCamera_Removed;
                    selectedCamera.Camera.ProfileUpdated += Camera_ProfileUpdated;
                    cameraState = selectedCamera.Camera.LumixState;
                    cameraState.PropertyChanged += Camera_PropertyChanged;
                    SetTime = DateTime.UtcNow;
                }
                else
                {
                    SetTime = DateTime.MinValue;
                }

                var task = RunAsync(() =>
                {
                    OnPropertyChanged(nameof(SelectedCamera));

                    RefreshAll();
                });
            }
        }

        public DateTime SetTime { get; private set; } = DateTime.MinValue;

        public ICollection<string> ShutterSpeeds
        {
            get
            {
                return cameraState?.MenuSet?.ShutterSpeeds.Select(s => s.Text).ToList() ?? new List<string>();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AsyncMenuItemSetter(ICameraMenuItem menu)
        {
            if (menu == null || SelectedCamera == null)
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await SelectedCamera.Camera.SendMenuItem(menu);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "AsyncSetter");
                }
            });
        }

        private void Camera_ProfileUpdated()
        {
            OnPropertyChanged(nameof(CanManualFocusAf));
        }

        private async void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await RunAsync(() =>
            {
                try
                {
                    switch (e.PropertyName)
                    {
                        case nameof(LumixState.FocusMode):
                            OnPropertyChanged(nameof(CanManualFocusAf));
                            OnPropertyChanged(nameof(CanResetTouchAf));
                            OnPropertyChanged(nameof(CanReleaseTouchAf));
                            OnPropertyChanged(nameof(FocusMode));
                            break;

                        case nameof(LumixState.CameraMode):
                            OnPropertyChanged(nameof(CanReleaseTouchAf));
                            OnPropertyChanged(nameof(CameraMode));
                            break;

                        case nameof(LumixState.CanManualFocus):
                            OnPropertyChanged(nameof(CanManualFocus));
                            break;

                        case nameof(LumixState.OpenedAperture):
                            OnPropertyChanged(nameof(Apertures));
                            break;

                        case nameof(LumixState.CanChangeShutter):
                            OnPropertyChanged(nameof(CanChangeShutter));
                            break;

                        case nameof(LumixState.CanChangeAperture):
                            OnPropertyChanged(nameof(CanChangeAperture));
                            break;

                        case nameof(LumixState.MenuSet):
                            OnPropertyChanged(nameof(ShutterSpeeds));
                            OnPropertyChanged(nameof(IsoValues));
                            OnPropertyChanged(nameof(Apertures));
                            break;

                        case nameof(LumixState.RecState):
                            OnPropertyChanged(nameof(RecState));
                            break;

                        case nameof(LumixState.CanCapture):
                            OnPropertyChanged(nameof(CanCapture));
                            break;

                        case nameof(LumixState.MaximumFocus):
                            OnPropertyChanged(nameof(MaximumFocus));
                            break;

                        case nameof(LumixState.CurrentFocus):
                            OnPropertyChanged(nameof(CurrentFocus));
                            break;

                        case nameof(LumixState.LensInfo):
                            OnPropertyChanged(nameof(CanPowerZoom));
                            OnPropertyChanged(nameof(MaxZoom));
                            OnPropertyChanged(nameof(MinZoom));
                            break;

                        case nameof(LumixState.Shutter):
                            OnPropertyChanged(nameof(CurrentShutter));
                            break;

                        case nameof(LumixState.Aperture):
                            OnPropertyChanged(nameof(CurrentAperture));
                            break;

                        case nameof(LumixState.Iso):
                            OnPropertyChanged(nameof(CurrentIso));
                            break;

                        case nameof(LumixState.Zoom):
                            OnPropertyChanged(nameof(CurentZoom));
                            break;

                        case nameof(LumixState.FocusAreas):
                            OnPropertyChanged(nameof(FocusAreas));
                            OnPropertyChanged(nameof(CanReleaseTouchAf));
                            OnPropertyChanged(nameof(CanResetTouchAf));
                            break;

                        case nameof(LumixState.AutoFocusMode):
                            OnPropertyChanged(nameof(AutoFocusMode));
                            OnPropertyChanged(nameof(CanReleaseTouchAf));
                            OnPropertyChanged(nameof(CanResetTouchAf));
                            break;

                        case nameof(LumixState.IsBusy):
                            OnPropertyChanged(nameof(IsConnectionActive));
                            break;

                        case nameof(LumixState.State):
                            OnPropertyChanged(nameof(BatteryCritical));
                            OnPropertyChanged(nameof(BatteryLevel));
                            OnPropertyChanged(nameof(GripBatteryLevel));
                            OnPropertyChanged(nameof(GripBatteryCritical));
                            OnPropertyChanged(nameof(GripBatteryPresent));
                            OnPropertyChanged(nameof(MemoryCardInfo));
                            OnPropertyChanged(nameof(MemoryCardAccess));
                            OnPropertyChanged(nameof(MemoryCardError));
                            OnPropertyChanged(nameof(MemoryCardInfo));
                            OnPropertyChanged(nameof(MemoryCard2Access));
                            OnPropertyChanged(nameof(MemoryCard2Error));
                            OnPropertyChanged(nameof(MemoryCardPresent));
                            OnPropertyChanged(nameof(MemoryCard2Present));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }

        private float GetBatteryLevel(string value)
        {
            var numbers = value?.Split('/');
            if (numbers?.Length != 2)
            {
                return -1;
            }

            if (!float.TryParse(numbers[0], out var val1)
                || !int.TryParse(numbers[1], out var val2) || val2 == 0)
            {
                return -1;
            }

            return val1 / val2;
        }

        private void RefreshAll()
        {
            try
            {
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsConnectionActive));

                OnPropertyChanged(nameof(CanResetTouchAf));
                OnPropertyChanged(nameof(CanReleaseTouchAf));
                OnPropertyChanged(nameof(CanManualFocusAf));
                OnPropertyChanged(nameof(CanChangeAperture));
                OnPropertyChanged(nameof(CanChangeShutter));
                OnPropertyChanged(nameof(CanManualFocus));
                OnPropertyChanged(nameof(CanPowerZoom));
                OnPropertyChanged(nameof(RecState));

                OnPropertyChanged(nameof(ShutterSpeeds));
                OnPropertyChanged(nameof(Apertures));
                OnPropertyChanged(nameof(IsoValues));

                OnPropertyChanged(nameof(CurrentAperture));
                OnPropertyChanged(nameof(CurrentShutter));
                OnPropertyChanged(nameof(CurrentIso));

                OnPropertyChanged(nameof(CanCapture));
                OnPropertyChanged(nameof(MaxZoom));
                OnPropertyChanged(nameof(MinZoom));
                OnPropertyChanged(nameof(CurentZoom));

                OnPropertyChanged(nameof(FocusAreas));
                OnPropertyChanged(nameof(BatteryLevel));
                OnPropertyChanged(nameof(MemoryCardInfo));
                OnPropertyChanged(nameof(MemoryCardError));
                OnPropertyChanged(nameof(MemoryCardAccess));
                OnPropertyChanged(nameof(CameraMode));
                OnPropertyChanged(nameof(FocusMode));
                OnPropertyChanged(nameof(AutoFocusMode));
                OnPropertyChanged(nameof(BatteryCritical));
                OnPropertyChanged(nameof(BatteryLevel));
                OnPropertyChanged(nameof(GripBatteryLevel));
                OnPropertyChanged(nameof(GripBatteryCritical));
                OnPropertyChanged(nameof(GripBatteryPresent));
                OnPropertyChanged(nameof(MemoryCard2Access));
                OnPropertyChanged(nameof(MemoryCard2Error));
                OnPropertyChanged(nameof(MemoryCardPresent));
                OnPropertyChanged(nameof(MemoryCard2Present));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async Task RunAsync(Action act)
        {
            if (Dispatcher != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => act());
            }
        }

        private void SelectedCamera_Removed()
        {
            var task = RunAsync(() =>
              {
                  SelectedCamera = null;
              });
        }
    }
}