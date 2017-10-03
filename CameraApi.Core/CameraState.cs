namespace CameraApi.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    public class CameraState
    {
        private string aperture;
        private CameraMode cameraMode = null;
        private float currentFocus;
        private float exposureShift;
        private FocusMode focusMode;
        private bool isBusy = true;
        private string iso;
        private int maximumFocus;
        private RecState recState;
        private string shutter;
        private int zoom;
        private bool canCapture;
        private bool canChangeAperture;
        private bool canChangeShutter;
        private bool canManualFocus;
        private bool isVideoMode;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Aperture { get; set; }

        public CameraMode CameraMode { get; set; }

        public bool CanCapture { get; set; }

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

        public ObservableCollection<IActionItem> Apertures { get; set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}