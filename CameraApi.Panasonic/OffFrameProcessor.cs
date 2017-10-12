namespace CameraApi.Panasonic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CameraApi.Panasonic.LumixData;
    using GMaster.Core.Tools;

    public class OffFrameProcessor
    {
        private readonly string deviceName;

        private readonly LumixState lumixState;
        private readonly CameraParser parser;

        private Slice lastPrint;

        public OffFrameProcessor(string deviceName, CameraParser parser, LumixState lumixState)
        {
            this.parser = parser;
            this.lumixState = lumixState;
            this.deviceName = deviceName;
        }

        public event Func<CancellationToken, Task> LensChanged;

        public bool OffFrameBytesSupported { get; } = true;

        public int CalcImageStart(Slice slice)
        {
            return slice.ToShort(30) + 32;
        }

        public void Process(Slice slice, IntPoint size)
        {
            try
            {
                var state = new ProcessState(slice, GetMultiplier(slice));

                if (!OffFrameBytesSupported || slice.Length < 130)
                {
                    return;
                }

                PrintBytes(slice);
                lumixState.Iso = GetFromShort(state.Main, 127, parser.IsoBinary);

                lumixState.Shutter = GetFromShort(state.Main, 68, parser.ShutterBinary);

                lumixState.Aperture = GetFromShort(state.Main, 56, parser.ApertureBinary);

                var newmode = state.Main[92].ToEnum(LumixCameraMode.Unknown);
                if (newmode != LumixCameraMode.VideoRecording)
                {
                    lumixState.CameraMode = newmode;
                }

                lumixState.Orientation = state.Original[42].ToEnum(CameraOrientation.Undefined);

                var newzoom = state.Main.ToShort(85);
                if (lumixState.Zoom == 0 && newzoom != 0)
                {
                    LensChanged?.Invoke();
                }

                lumixState.Zoom = newzoom;

                lumixState.ExposureShift = state.Main.ToShort(128);

                lumixState.FocusAreas = GetFocusPoint(state.Original, size);

                lumixState.FocusMode = state.Main[107].ToEnum(LumixFocusMode.Unknown);

                lumixState.AutoFocusMode = state.Main[109].ToEnum(LumixAutoFocusMode.Unknown);
            }
            catch (Exception e)
            {
                Log.Error(new Exception("Cannot parse off-frame bytes for camera: " + deviceName, e));
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

        private static FocusAreas GetFocusPoint(Slice slice, IntPoint size)
        {
            var pointsNum = slice[47];
            var focusSlice = new Slice(slice, 48);
            var multiplier = GetMultiplier(slice);
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

        private static int GetMultiplier(Slice slice)
        {
            return slice[46] == 0xff ? 12 : 16;
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

        private void PrintBytes(Slice slice)
        {
            Debug.WriteLine(
                () =>
                    {
                        if (lastPrint == null || slice.Length != lastPrint.Length)
                        {
                            lastPrint = slice;
                            return string.Join(",", slice.Skip(32).Select(a => a.ToString("X2")));
                        }

                        var str = new StringBuilder();
                        for (int i = 0; i < slice.Length; i++)
                        {
                            str.Append(slice[i].ToString("X2"));
                            if (i < slice.Length - 1)
                            {
                                str.Append(slice[i] != lastPrint[i] || slice[i + 1] != lastPrint[i + 1] ? '#' : ',');
                            }
                        }
                        lastPrint = slice;
                        return str.ToString();
                    }, "OffFrameBytes");
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