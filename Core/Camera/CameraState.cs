namespace GMaster.Core.Camera
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;

    public class CameraState : INotifyPropertyChanged
    {
        private string aperture;
        private AutoFocusMode autoFocusMode;
        private CameraMode cameraMode = CameraMode.Unknown;
        private float currentFocus;
        private float exposureShift;
        private FocusMode focusMode;
        private FocusAreas focusPoints;
        private bool isBusy = true;
        private string iso;
        private int maximumFocus;
        private CameraOrientation orientation;
        private RecState recState;
        private string shutter;
        private int zoom;
        private bool canCapture;
        private bool canChangeAperture;
        private bool canChangeShutter;
        private bool canManualFocus;
        private bool isVideoMode;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Aperture
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
            }
        }

        public bool CanCapture
        {
            get => canCapture;
            set
            {
                if (value == canCapture)
                {
                    return;
                }

                canCapture = value;
                OnPropertyChanged();
            }
        }

        public bool CanChangeAperture
        {
            get => canChangeAperture;

            set
            {
                if (value == canChangeAperture)
                {
                    return;
                }

                canChangeAperture = value;
                OnPropertyChanged();
            }
        }

        public bool CanChangeShutter
        {
            get => canChangeShutter;

            set
            {
                if (value == canChangeShutter)
                {
                    return;
                }

                canChangeShutter = value;
                OnPropertyChanged();
            }
        }

        public bool CanManualFocus
        {
            get => canManualFocus;

            set
            {
                if (value == canManualFocus)
                {
                    return;
                }

                canManualFocus = value;
                OnPropertyChanged();
            }
        }

        public float CurrentFocus
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

        public float ExposureShift
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

        public string Iso
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

        public bool IsVideoMode
        {
            get => isVideoMode;

            set
            {
                if (value == isVideoMode)
                {
                    return;
                }

                isVideoMode = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public string Shutter
        {
            get => shutter;
            set
            {
                if (shutter == value)
                {
                    return;
                }

                shutter = value;
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}