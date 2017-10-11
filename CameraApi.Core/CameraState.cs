namespace CameraApi.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    public interface ICameraState : INotifyPropertyChanged
    {
        string Aperture { get; }

        string CameraMode { get; }

        bool CanCapture { get; }

        bool CanChangeAperture { get; }

        bool CanChangeShutter { get; }

        bool CanManualFocus { get; }

        float Focus { get; }

        float ExposureShift { get; }

        string FocusMode { get; }

        bool IsBusy { get; }

        string Iso { get; }

        bool IsVideoMode { get; }

        int MaximumFocus { get; }

        RecState RecState { get; }

        string Shutter { get; }

        int Zoom { get; }

        ObservableCollection<IActionItem> Apertures { get; }

        ObservableCollection<IActionItem> Shutters { get; }
    }
}