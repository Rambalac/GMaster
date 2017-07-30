namespace GMaster.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Core.Camera;
    using Core.Tools;
    using Core.Camera.Panasonic;
    using Core.Camera.Panasonic.LumixData;
    using Windows.UI.Core;

    public class CameraViewModel : INotifyPropertyChanged
    {
        private int lastFocusAreasCount;
        private LumixState lumixState;
        private ConnectedCamera selectedCamera;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollection<string> Apertures
        {
            get
            {
                if (lumixState?.LensInfo == null)
                {
                    return new List<string>();
                }

                var openedAperture = lumixState.OpenedAperture;
                return new[] { openedAperture }
                    .Concat(lumixState.MenuSet.Apertures.
                            Where(a => a.IntValue <= lumixState.LensInfo.ClosedAperture &&
                                    a.IntValue > openedAperture.IntValue))
                    .Select(a => a.Text).ToList();
            }
        }

        public AutoFocusMode AutoFocusMode => lumixState?.AutoFocusMode ?? AutoFocusMode.Unknown;

        public bool BatteryCritical => Math.Abs(BatteryLevel) < 0.01;

        public bool GripBatteryCritical => Math.Abs(GripBatteryLevel) < 0.01;

        public bool GripBatteryPresent => GripBatteryLevel >= 0;

        public float BatteryLevel => GetBatteryLevel(lumixState?.State?.Battery);

        public float GripBatteryLevel => GetBatteryLevel(lumixState?.State?.GripBattery);

        public CameraMode CameraMode => lumixState?.CameraMode ?? CameraMode.Unknown;

        public bool CanCapture => lumixState?.CanCapture ?? false;

        public bool CanChangeAperture => lumixState?.CanChangeAperture ?? true;

        public bool CanChangeShutter => lumixState?.CanChangeShutter ?? true;

        public bool CanManualFocus => lumixState?.CanManualFocus ?? false;

        public bool CanManualFocusAf => (lumixState?.FocusMode ?? FocusMode.Unknown) == FocusMode.MF && (selectedCamera?.Camera.Profile.ManualFocusAF ?? false);

        public bool CanPowerZoom => lumixState?.LensInfo?.HasPowerZoom ?? false;

        public bool CanReleaseTouchAf => (AutoFocusMode.ToValue<AutoFocusModeFlags>().HasFlag(AutoFocusModeFlags.TouchAFRelease)
                                            && FocusAreas != null && FocusAreas.Boxes.Count > 0)
                                         || lumixState?.CameraMode == CameraMode.MFAssist
                                         || (FocusAreas?.Boxes.Any(b => b.Props.Type == FocusAreaType.MfAssistPinP || b.Props.Type == FocusAreaType.MfAssistFullscreen) ?? false);

        public bool CanResetTouchAf => (lumixState?.FocusMode ?? FocusMode.Unknown) == FocusMode.MF ||
                                       (FocusAreas?.Boxes.Any(b => b.Props.Type == FocusAreaType.OneAreaSelected || b.Props.Type == FocusAreaType.FaceOther) ?? false);

        public int CurentZoom => lumixState?.Zoom ?? 0;

        public string CurrentAperture
        {
            get => lumixState?.Aperture.Text;

            set
            {
                AsyncMenuItemSetter(lumixState.MenuSet.Apertures.SingleOrDefault(a => a.Text == value) ?? lumixState.OpenedAperture);
            }
        }

        public int CurrentFocus => lumixState?.CurrentFocus ?? 0;

        public string CurrentIso
        {
            get
            {
                if (lumixState?.Iso.Text == null)
                {
                    return null;
                }

                return lumixState.MenuSet.IsoValues.FirstOrDefault(i => i.Text.EndsWith(lumixState.Iso.Text, StringComparison.OrdinalIgnoreCase)).Text;
            }

            set
            {
                AsyncMenuItemSetter(lumixState?.MenuSet?.IsoValues.SingleOrDefault(a => a.Text == value));
            }
        }

        public string CurrentShutter
        {
            get => lumixState?.Shutter.Text;

            set
            {
                Debug.WriteLine("Shutter set to: " + value, "LumixState");

                AsyncMenuItemSetter(lumixState?.MenuSet?.ShutterSpeeds.SingleOrDefault(a => a.Text == value));
            }
        }

        public CoreDispatcher Dispatcher { get; set; }

        public FocusAreas FocusAreas
        {
            get
            {
                var newval = lumixState?.FocusAreas;
                if (lastFocusAreasCount != (newval?.Boxes.Count ?? 0))
                {
                    lastFocusAreasCount = newval?.Boxes.Count ?? 0;
                    OnPropertyChanged(nameof(CanReleaseTouchAf));
                }

                return newval;
            }
        }

        public FocusMode FocusMode => lumixState?.FocusMode ?? FocusMode.Unknown;

        public bool IsConnected => selectedCamera != null;

        public bool IsConnectionActive => !(lumixState?.IsBusy ?? true);

        public ICollection<string> IsoValues
        {
            get
            {
                return lumixState?.MenuSet?.IsoValues
                    .Where(i => lumixState.CurMenu.Enabled.ContainsKey(i.Id)).Select(i => i.Text).ToList()
                    ?? new List<string>();
            }
        }

        public int MaximumFocus => lumixState?.MaximumFocus ?? 0;

        public int MaxZoom => lumixState?.LensInfo?.MaxZoom ?? 0;

        public bool MemoryCardAccess => lumixState?.State.SdAccess == OnOff.On;

        public bool MemoryCard2Access => lumixState?.State.Sd2Access == OnOff.On;

        public bool MemoryCardError => lumixState != null && (lumixState.State.SdMemory == SdMemorySet.Unset
                                                              || lumixState.State.SdCardStatus != SdCardStatus.WriteEnable);

        public bool MemoryCard2Error => lumixState != null && (lumixState.State.Sd2Memory == SdMemorySet.Unset
                                                              || lumixState.State.Sd2CardStatus != SdCardStatus.WriteEnable);

        public bool MemoryCardPresent => (lumixState?.State?.SdMemory ?? SdMemorySet.Unset) == SdMemorySet.Set;

        public bool MemoryCard2Present => (lumixState?.State?.Sd2Memory ?? SdMemorySet.Unset) == SdMemorySet.Set;

        public string MemoryCardInfo
        {
            get
            {
                if (lumixState?.State == null)
                {
                    return string.Empty;
                }

                if (lumixState.State.SdMemory == SdMemorySet.Unset
                    && lumixState.State.Sd2Memory == SdMemorySet.Unset)
                {
                    return "Not inserted";
                }

                if (lumixState.State.SdCardStatus != SdCardStatus.WriteEnable
                    && lumixState.State.Sd2CardStatus != SdCardStatus.WriteEnable)
                {
                    return "Read Only";
                }

                if (lumixState.State.RemainDisplayType == RemainDisplayType.Time)
                {
                    return TimeSpan.FromSeconds(lumixState.State.VideoRemainCapacity).ToString("hh'h:'mm'm:'ss's'").TrimStart('0', 'h', 'm', ':');
                }

                return lumixState.State.RemainCapacity.ToString();
            }
        }

        public int MinZoom => lumixState?.LensInfo?.MinZoom ?? 0;

        public RecState RecState => lumixState?.RecState ?? RecState.Stopped;

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
                    lumixState.PropertyChanged -= Camera_PropertyChanged;
                    selectedCamera.Removed -= SelectedCamera_Removed;
                    selectedCamera.Camera.ProfileUpdated -= Camera_ProfileUpdated;
                }

                selectedCamera = value;

                if (selectedCamera != null)
                {
                    selectedCamera.Removed += SelectedCamera_Removed;
                    selectedCamera.Camera.ProfileUpdated += Camera_ProfileUpdated;
                    lumixState = selectedCamera.Camera.LumixState;
                    lumixState.PropertyChanged += Camera_PropertyChanged;
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
                return lumixState?.MenuSet?.ShutterSpeeds.Select(s => s.Text).ToList() ?? new List<string>();
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