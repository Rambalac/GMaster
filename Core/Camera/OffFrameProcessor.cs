namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using LumixData;
    using Tools;

    public enum FocusAreaType
    {
        OneAreaSelected = 0x0001,
        FaceOther = 0xff01,
        MainFace = 0x0002,
        Eye = 0xff09,
        TrackUnlock = 0xff03,
        TrackLock = 0x0003,

        Box = 0xff02,
        Cross = 0xff04
    }

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

        public int CalcImageStart(Slice slice)
        {
            return slice.ToShort(30) + 32;
        }

        public void Process(Slice slice, CameraPoint size)
        {
            try
            {
                var state = new ProcessState(slice, GetMultiplier(slice));

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

        private static int GetMultiplier(Slice slice)
        {
            return slice[46] == 0xff ? 12 : 16;
        }

        private FocusAreas GetFocusPoint(Slice slice, CameraPoint size)
        {
            var pointsNum = slice[47];
            var focusSlice = new Slice(slice, 48);
            int multiplier = GetMultiplier(slice);
            if (pointsNum > 0)
            {
                var result = new FocusAreas(pointsNum, size, slice[46] == 0xff);

                for (var i = 0; i < pointsNum; i++)
                {
                    var x1 = focusSlice.ToShort(0 + (i * multiplier));
                    var y1 = focusSlice.ToShort(2 + (i * multiplier));
                    var x2 = focusSlice.ToShort(4 + (i * multiplier));
                    var y2 = focusSlice.ToShort(6 + (i * multiplier));
                    var typeval = (int)focusSlice.ToUShort(10 + (i * multiplier));
                    var type = Enum.IsDefined(typeof(FocusAreaType), typeval) ? (FocusAreaType)typeval : FocusAreaType.FaceOther;
                    var failed = focusSlice[9 + (i * multiplier)] == 0;
                    result.AddBox(x1, y1, x2, y2, type, failed);
                }

                return result;
            }

            return null;
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

        private class ProcessState
        {
            public ProcessState(Slice array, int multiplier)
            {
                Original = array;

                Main = new Slice(array, Original[47] * multiplier);
            }

            public Slice Main { get; }

            public Slice Original { get; }
        }
    }
}