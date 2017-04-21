namespace GMaster.Core.Camera
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;
    using LumixData;
    using Tools;

    public class LumixState : INotifyPropertyChanged
    {
        private TextBinValue aperture;
        private CameraMode cameraMode = CameraMode.Unknown;
        private CurMenu curMenu;
        private int currentFocus;
        private int exposureShift;
        private FocusMode focusMode;
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
        private AutoFocusMode autoFocusMode;

        public event PropertyChangedEventHandler PropertyChanged;

        public TextBinValue Aperture
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

        public CameraMode CameraMode
        {
            get => cameraMode;
            set
            {
                if (value == cameraMode)
                {
                    return;
                }

                cameraMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanChangeAperture));
                OnPropertyChanged(nameof(CanChangeShutter));
                OnPropertyChanged(nameof(CanCapture));
                OnPropertyChanged(nameof(IsVideoMode));
            }
        }

        public bool CanCapture => CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Photo);

        public bool CanChangeAperture => CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Aperture);

        public bool CanChangeShutter => CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Shutter);

        public bool CanManualFocus => FocusMode == FocusMode.Manual;

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

        public FocusMode FocusMode
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

        public bool IsVideoMode => CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Video);

        public LensInfo LensInfo
        {
            get => lensInfo;
            set
            {
                if (Equals(value, lensInfo))
                {
                    return;
                }

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

        public AutoFocusMode AutoFocusMode
        {
            get => autoFocusMode;
            set
            {
                if (value == autoFocusMode)
                {
                    return;
                }

                autoFocusMode = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Aperture = default(TextBinValue);
            Shutter = default(TextBinValue);
            Iso = default(TextBinValue);
            CameraMode = CameraMode.Unknown;
            FocusAreas = null;
            FocusMode = FocusMode.Unknown;
            LensInfo = null;
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