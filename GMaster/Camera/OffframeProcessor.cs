namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Logger;
    using LumixData;
    using Tools;

    public class OffFrameProcessor : INotifyPropertyChanged
    {
        private readonly string deviceName;

        private readonly CameraParser parser;

        public OffFrameProcessor(string deviceName, CameraParser parser)
        {
            this.parser = parser;
            this.deviceName = deviceName;
        }

        public event Action LensUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public TextBinValue Aperture { get; private set; }

        public CameraMode CameraMode { get; private set; } = CameraMode.Unknown;

        public int ExposureShift { get; private set; }

        public FocusMode FocusMode { get; private set; }

        public FocusAreas FocusPoints { get; private set; }

        public TextBinValue Iso { get; private set; }

        public bool OffframeBytesSupported { get; } = true;

        public int OpenedAperture { get; private set; }

        public CameraOrientation Orientation { get; private set; }

        public TextBinValue Shutter { get; private set; }

        public int Zoom { get; private set; }

        public void Process(Slice slice, CameraPoint size)
        {
            try
            {
                var state = new ProcessState(slice);

                if (!OffframeBytesSupported || slice.Length < 130)
                {
                    return;
                }

                Debug.WriteLine(() => string.Join(",", slice.Skip(32).Select(a => a.ToString("X2"))), "OffFrameBytes");
                var newIso = GetFromShort(state.Main, 127, parser.IsoBinary);
                if (!Equals(newIso, Iso))
                {
                    Iso = newIso;
                    OnPropertyChanged(nameof(Iso));
                }

                var newShutter = GetFromShort(state.Main, 68, parser.ShutterBinary);
                if (!Equals(newShutter, Shutter))
                {
                    Shutter = newShutter;
                    OnPropertyChanged(nameof(Shutter));
                }

                var newOpenedAperture = state.Main.ToShort(52);
                if (newOpenedAperture != OpenedAperture)
                {
                    OpenedAperture = newOpenedAperture;
                    OnPropertyChanged(nameof(OpenedAperture));
                }

                var newAperture = GetFromShort(state.Main, 56, parser.ApertureBinary);
                if (!Equals(newAperture, Aperture))
                {
                    Aperture = newAperture;
                    OnPropertyChanged(nameof(Aperture));
                }

                var newMode = state.Main[92].ToEnum(CameraMode.Unknown);
                if (newMode != CameraMode)
                {
                    CameraMode = newMode;
                    OnPropertyChanged(nameof(CameraMode));
                }

                var newOrient = state.Original[42].ToEnum(CameraOrientation.Undefined);
                if (newOrient != Orientation)
                {
                    Orientation = newOrient;
                    OnPropertyChanged(nameof(Orientation));
                }

                var newZoom = (int)state.Main.ToShort(85);
                if (newZoom != Zoom)
                {
                    if (Zoom == 0 || newZoom == 0)
                    {
                        LensUpdated?.Invoke();
                    }

                    Zoom = newZoom;
                    OnPropertyChanged(nameof(Zoom));
                }

                var newExposureShift = (int)state.Main.ToShort(128);
                if (newExposureShift != ExposureShift)
                {
                    ExposureShift = newExposureShift;
                    OnPropertyChanged(nameof(ExposureShift));
                }

                var newFocusPoints = GetFocusPoint(state.Original, size);
                if (!Equals(newFocusPoints, FocusPoints))
                {
                    FocusPoints = newFocusPoints;
                    OnPropertyChanged(nameof(FocusPoints));
                }

                var newFocusMode = state.Main[107].ToEnum(FocusMode.Unknown);
                if (newFocusMode != FocusMode)
                {
                    FocusMode = newFocusMode;
                    OnPropertyChanged(nameof(FocusMode));
                }
            }
            catch (Exception e)
            {
                Log.Error(new Exception("Cannot parse off-frame bytes for camera: " + deviceName, e));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static string FindClosest(IReadOnlyDictionary<int, string> dict, int value)
        {
            var list = dict.Keys.ToList();
            list.Sort();
            var index = list.BinarySearch(value);
            if (index > 0)
            {
                return dict[list[index]];
            }

            index = ~index;
            if (index >= list.Count)
            {
                return dict[list[list.Count - 1]];
            }

            if (index <= 0)
            {
                return dict[list[0]];
            }

            var val1 = list[index - 1];
            var val2 = list[index];
            return val2 - value > value - val1 ? dict[val1] : dict[val2];
        }

        private TextBinValue GetFromShort(Slice slice, int index, IReadOnlyDictionary<int, string> dict)
        {
            var bin = slice.ToShort(index);
            try
            {
                if (dict.TryGetValue(bin, out var val))
                {
                    return new TextBinValue(val, bin);
                }

                val = FindClosest(dict, bin);
                return new TextBinValue(val, bin);
            }
            catch (KeyNotFoundException e)
            {
                Log.Error(new Exception("Cannot parse off-frame bytes for camera: " + deviceName, e));
                return new TextBinValue("!", bin);
            }
        }

        private FocusAreas GetFocusPoint(Slice slice, CameraPoint size)
        {
            var pointsNum = slice[47];
            if (pointsNum > 0)
            {
                var result = new FocusAreas(pointsNum, size, parser is GH4Parser);

                for (var i = 0; i < pointsNum; i++)
                {
                    result.AddBox(
                        slice.ToShort(48 + (i * 16)),
                        slice.ToShort(50 + (i * 16)),
                        slice.ToShort(52 + (i * 16)),
                        slice.ToShort(54 + (i * 16)));
                }

                return result;
            }

            return null;
        }

        private class ProcessState
        {
            public ProcessState(Slice array)
            {
                Original = array;

                Main = new Slice(array, Original[47] * 16);
            }

            public Slice Main { get; }

            public Slice Original { get; }
        }
    }
}