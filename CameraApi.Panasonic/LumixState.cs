namespace CameraApi.Panasonic
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using CameraApi.Core;
    using CameraApi.Panasonic.LumixData;
    using GMaster.Core.Tools;
    using JetBrains.Annotations;

    public class LumixState : ICameraState
    {
        private TextBinValue aperture;
        private LumixAutoFocusMode lumixAutoFocusMode;
        private LumixCameraMode lumixCameraMode = LumixCameraMode.Unknown;
        private CurMenu curMenu;
        private int currentFocus;
        private int exposureShift;
        private LumixFocusMode focusMode;
        private FocusAreas focusPoints;
        private bool isBusy = true;
        private TextBinValue iso;
        private LensInfo lensInfo;
        private int maximumFocus;
        private MenuSet menuSet;
        private CameraOrientation orientation;
        private RecState recState;
        private TextBinValue shutter;
        private CameraState state;
        private int zoom;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Aperture
        {
            get => aperture.Text;
        }

        public TextBinValue LumixAperture
        {
            get => aperture;
            set
            {
                if (value.Equals(aperture))
                {
                    return;
                }

                aperture = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<LumixAutoFocusMode, AutoFocusMode> ToAutoFocusMode = new Dictionary<LumixAutoFocusMode, AutoFocusMode>
        {
            { LumixAutoFocusMode.Face, AutoFocusMode.Face}
        };

        public AutoFocusMode AutoFocusMode
        {
            get => ToAutoFocusMode[lumixAutoFocusMode];
        }

        public LumixAutoFocusMode LumixAutoFocusMode
        {
            get => lumixAutoFocusMode;
            set
            {
                if (value == lumixAutoFocusMode)
                {
                    return;
                }

                lumixAutoFocusMode = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<LumixCameraMode, CameraMode> ToCameraMode = new Dictionary<LumixCameraMode, CameraMode>
        {
            { LumixCameraMode.A, CameraMode.A},
            { LumixCameraMode.S, CameraMode.S},
            { LumixCameraMode.M, CameraMode.M},
            { LumixCameraMode.iA, CameraMode.iA},
            { LumixCameraMode.vA, CameraMode.vA},
            { LumixCameraMode.vS, CameraMode.vS},
            { LumixCameraMode.vM, CameraMode.vM},
        };

        public CameraMode CameraMode
        {
            get => ToCameraMode[lumixCameraMode];
        }

        public LumixCameraMode LumixCameraMode
        {
            get => lumixCameraMode;
            set
            {
                if (value == lumixCameraMode)
                {
                    return;
                }

                lumixCameraMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanChangeAperture));
                OnPropertyChanged(nameof(CanChangeShutter));
                OnPropertyChanged(nameof(CanCapture));
                OnPropertyChanged(nameof(IsVideoMode));
            }
        }

        public bool CanCapture => CameraMode.ToValue<LumixCameraModeFlags>().HasFlag(LumixCameraModeFlags.Photo);

        public bool CanChangeAperture => CameraMode.ToValue<LumixCameraModeFlags>().HasFlag(LumixCameraModeFlags.Aperture);

        public bool CanChangeShutter => CameraMode.ToValue<LumixCameraModeFlags>().HasFlag(LumixCameraModeFlags.Shutter);

        public bool CanManualFocus => FocusMode == FocusMode.MF;

        public CurMenu CurMenu
        {
            get => curMenu;
            set
            {
                curMenu = value;
                OnPropertyChanged();
            }
        }

        public int CurrentFocus
        {
            get => currentFocus;
            set
            {
                if (value == currentFocus)
                {
                    return;
                }

                currentFocus = value;
                OnPropertyChanged();
            }
        }

        public int ExposureShift
        {
            get => exposureShift;
            set
            {
                if (value == exposureShift)
                {
                    return;
                }

                exposureShift = value;
                OnPropertyChanged();
            }
        }

        public FocusAreas FocusAreas
        {
            get => focusPoints;
            set
            {
                if (Equals(value, focusPoints))
                {
                    return;
                }

                focusPoints = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<LumixFocusMode, FocusMode> ToFocusMode = new Dictionary<LumixFocusMode, FocusMode>
        {
            {LumixFocusMode.AFC, FocusMode.AFC }
        };

        public FocusMode FocusMode
        {
            get => ToFocusMode[focusMode];

        }

        public LumixFocusMode LumixFocusMode
        {
            get => focusMode;
            set
            {
                if (value == focusMode)
                {
                    return;
                }

                focusMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanManualFocus));
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (value == isBusy)
                {
                    return;
                }

                isBusy = value;
                OnPropertyChanged();
            }
        }

        public bool IsLimited { get; set; }

        public TextBinValue Iso
        {
            get => iso;
            set
            {
                if (value.Equals(iso))
                {
                    return;
                }

                iso = value;
                OnPropertyChanged();
            }
        }

        public bool IsVideoMode => CameraMode.ToValue<LumixCameraModeFlags>().HasFlag(LumixCameraModeFlags.Video);

        public LensInfo LensInfo
        {
            get => lensInfo;
            set
            {
                lensInfo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpenedAperture));
            }
        }

        public int MaximumFocus
        {
            get => maximumFocus;
            set
            {
                if (value == maximumFocus)
                {
                    return;
                }

                maximumFocus = value;
                OnPropertyChanged();
            }
        }

        public MenuSet MenuSet
        {
            get => menuSet;
            set
            {
                menuSet = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Aperture));
                OnPropertyChanged(nameof(Shutter));
                OnPropertyChanged(nameof(Iso));
            }
        }

        public CameraMenuItem256 OpenedAperture
        {
            get
            {
                var open = LensInfo.OpenedAperture;
                var opentext = CameraParser.ApertureBinToText(open);
                return new CameraMenuItem256(open.ToString(), opentext, "setsetting", "focal", open);
            }
        }

        public CameraOrientation Orientation
        {
            get => orientation;
            set
            {
                if (value == orientation)
                {
                    return;
                }

                orientation = value;
                OnPropertyChanged();
            }
        }

        public RecState RecState
        {
            get => recState;
            set
            {
                if (value == recState)
                {
                    return;
                }

                recState = value;
                Debug.WriteLine("Ser RecState: " + value, "RecState");
                OnPropertyChanged();
            }
        }

        public TextBinValue Shutter
        {
            get => shutter;
            set
            {
                if (shutter.Text == value.Text)
                {
                    return;
                }

                shutter = value;
                OnPropertyChanged();
            }
        }

        public CameraState State
        {
            get => state;
            set
            {
                if (Equals(value, state))
                {
                    return;
                }

                state = value;
                OnPropertyChanged();
            }
        }

        public int Zoom
        {
            get => zoom;
            set
            {
                if (value == zoom)
                {
                    return;
                }

                zoom = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Aperture = "";
            Shutter = default(TextBinValue);
            Iso = default(TextBinValue);
            LumixCameraMode = LumixCameraMode.Unknown;
            FocusAreas = null;
            LumixFocusMode = LumixCameraMode.Unknown;
            Zoom = 0;
            RecState = RecState.Unknown;
            Orientation = CameraOrientation.Undefined;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}