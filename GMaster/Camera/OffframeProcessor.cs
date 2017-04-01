namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;

    public class OffframeProcessor : INotifyPropertyChanged
    {
        private static readonly IReadOnlyDictionary<int, CameraMode> IntToCameraMode = BuildEnumDict<CameraMode>();

        private static readonly IReadOnlyDictionary<int, CameraOrientation> IntToOrient = BuildEnumDict<CameraOrientation>();

        private readonly string deviceName;

        private readonly CameraParser parser;

        public OffframeProcessor(string deviceName, CameraParser parser)
        {
            this.parser = parser;
            this.deviceName = deviceName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TextBinValue Aperture { get; private set; }

        public CameraMode CameraMode { get; private set; }

        public int ClosedAperture { get; private set; }

        public FocusPoint FocusPoint { get; private set; }

        public TextBinValue Iso { get; private set; }

        public bool OffframeBytesSupported { get; private set; } = true;

        public int OpenedAperture { get; private set; }

        public CameraOrientation Orientation { get; private set; }

        public TextBinValue Shutter { get; private set; }

        public int Zoom { get; private set; }

        public void Process(MemoryStream offframeBytes)
        {
            try
            {
                if (!OffframeBytesSupported || offframeBytes == null || offframeBytes.Length < 130 || !offframeBytes.TryGetBuffer(out var array))
                {
                    return;
                }

                Debug.WriteLine(string.Join(",", array.Select(a => a.ToString("X2"))));

                var state = new ProcessState(array);

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
                    Debug.WriteLine($"Open {newOpenedAperture}");
                    OpenedAperture = newOpenedAperture;
                    OnPropertyChanged(nameof(OpenedAperture));
                }

                var newAperture = GetFromShort(state.Main, 56, parser.ApertureBinary);
                if (!Equals(newAperture, Aperture))
                {
                    Aperture = newAperture;
                    OnPropertyChanged(nameof(Aperture));
                }

                var newMode = IntToCameraMode.TryGetValue(state.Main[92], out var mode) ? mode : CameraMode.Unknown;
                if (newMode != CameraMode)
                {
                    CameraMode = newMode;
                    OnPropertyChanged(nameof(CameraMode));
                }

                IntToOrient.TryGetValue(state.Original[92], out var newOrient);
                if (newOrient != Orientation)
                {
                    Orientation = newOrient;
                    OnPropertyChanged(nameof(Orientation));
                }

                var newZoom = (int)state.Main.ToShort(85);
                if (newZoom != Zoom)
                {
                    Zoom = newZoom;
                    OnPropertyChanged(nameof(Zoom));
                }

                var newFocusPoint = GetPointZoom(state.Original);
                if (!Equals(newFocusPoint, FocusPoint))
                {
                    FocusPoint = newFocusPoint;
                    OnPropertyChanged(nameof(FocusPoint));
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
            App.RunAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        private static IReadOnlyDictionary<int, T> BuildEnumDict<T>()
            where T : struct, IConvertible
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToImmutableDictionary(m => m.ToInt32(null));
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
            return (val2 - value > value - val1) ? dict[val1] : dict[val2];
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

        private FocusPoint GetPointZoom(Slice slice)
        {
            var t = slice[45];
            if (t == 0x41 || t == 0x01)
            {
                return new FocusPoint
                {
                    X1 = slice.ToShort(48) / 1000.0,
                    Y1 = slice.ToShort(50) / 1000.0,
                    X2 = slice.ToShort(52) / 1000.0,
                    Y2 = slice.ToShort(54) / 1000.0
                };
            }

            return null;
        }

        private class ProcessState
        {
            public ProcessState(ArraySegment<byte> array)
            {
                Original = new Slice(array);

                Main = new Slice(array, Original[47] * 16);
            }

            public Slice Main { get; }

            public Slice Original { get; }
        }

        private class Slice
        {
            private readonly ArraySegment<byte> array;
            private readonly int offset;

            public Slice(ArraySegment<byte> array, int offset = 0)
            {
                this.array = array;
                this.offset = offset + array.Offset;
            }

            public byte this[int index] => array.Array[offset + index];

            public short ToShort(int i)
            {
                return (short)((((int)this[i]) << 8) + this[i + 1]);
            }
        }
    }
}