using System.Linq;

namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;

    public class MenuSet
    {
        public static IReadOnlyDictionary<int, string> ApertureBinary { get; } = new Dictionary<int, string>
        {
            { 392, "1.7" },
            { 427, "1.8" },
            { 512, "2" },
            { 598, "2.2" },
            { 683, "2.5" },
            { 768, "2.8" },
            { 854, "3.2" },
            { 938, "3.5" },
            { 1024, "4" },
            { 1110, "4.5" },
            { 1195, "5" },
            { 1280, "5.6" },
            { 1366, "6.3" },
            { 1451, "7.1" },
            { 1536, "8" },
            { 1622, "9" },
            { 1707, "10" },
            { 1792, "11" },
            { 1878, "13" },
            { 1963, "14" },
            { 2048, "16" }
        };

        public static IReadOnlyDictionary<int, string> ShutterBinary { get; } = new Dictionary<int, string>
        {
            { 3072, "4000" },
            { 2987, "3200" },
            { 2902, "2500" },
            { 2816, "2000" },
            { 2731, "1600" },
            { 2646, "1300" },
            { 2560, "1000" },
            { 2475, "800" },
            { 2390, "640" },
            { 2304, "500" },
            { 2219, "400" },
            { 2134, "320" },
            { 2048, "250" },
            { 1963, "200" },
            { 1878, "160" },
            { 1792, "125" },
            { 1707, "100" },
            { 1622, "80" },
            { 1536, "60" },
            { 1451, "50" },
            { 1366, "40" },
            { 1280, "30" },
            { 1195, "25" },
            { 1110, "20" },
            { 1024, "15" },
            { 939, "13" },
            { 854, "10" },
            { 768, "8" },
            { 683, "6" },
            { 598, "5" },
            { 512, "4" },
            { 427, "3.2" },
            { 342, "2.5" },
            { 256, "2" },
            { 171, "1.6" },
            { 86, "1.3" },
            { 0, "1" },
            { -85, "1.3ˮ" },
            { -170, "1.6ˮ" },
            { -256, "2ˮ" },
            { -341, "2.5ˮ" },
            { -426, "3.2ˮ" },
            { -512, "4ˮ" },
            { -600, "5ˮ" },
            { -682, "6ˮ" },
            { -768, "8ˮ" },
            { -853, "10ˮ" },
            { -938, "13ˮ" },
            { -1024, "15ˮ" },
            { -1109, "20ˮ" },
            { -1194, "25ˮ" },
            { -1280, "30ˮ" },
            { -1365, "40ˮ" },
            { -1450, "50ˮ" },
            { -1536, "60ˮ" },
            { 16384, "B" }
        };

        public static IReadOnlyDictionary<int, string> IsoBinary { get; } = new Dictionary<int, string>
        {
            { 7167, "25600" },
            { 6911, "20000" },
            { 6655, "16000" },
            { 6399, "12800" },
            { 5887, "10000" },
            { 5375, "8000" },
            { 5119, "6400" },
            { 4863, "5000" },
            { 4607, "4000" },
            { 4351, "3200" },
            { 4095, "2500" },
            { 3839, "2000" },
            { 3583, "1600" },
            { 3327, "1250" },
            { 3071, "1000" },
            { 2815, "800" },
            { 2559, "640" },
            { 2303, "500" },
            { 2047, "400" },
            { 1791, "320" },
            { 1535, "250" },
            { 1279, "200" },
            { 1023, "160" },
            { 767,  "125" },
            { -1, "Auto" }
        };

        public TitledList<CameraMenuItem> AutofocusModes { get; set; }

        public TitledList<CameraMenuItem> BurstModes { get; set; }

        public TitledList<CameraMenuItem> CreativeControls { get; set; }

        public TitledList<CameraMenuItem> CustomMultiModes { get; set; }

        public TitledList<CameraMenuItem> DbValues { get; set; }

        public TitledList<CameraMenuItem> ExposureShifts { get; set; }

        public TitledList<CameraMenuItem> IsoValues { get; set; }

        public TitledList<CameraMenuItem> LiveviewQuality { get; set; }

        public TitledList<CameraMenuItem> MeteringMode { get; set; }

        public TitledList<CameraMenuItem> PeakingModes { get; set; }

        public TitledList<CameraMenuItem> PhotoQuality { get; set; }

        public TitledList<CameraMenuItem> PhotoSizes { get; set; }

        public TitledList<CameraMenuItem> PhotoStyles { get; set; }

        public TitledList<CameraMenuItem> PhotoAspects { get; set; }

        public TitledList<CameraMenuItem> Angles { get; set; }

        public static TitledList<CameraMenuItem> ShutterSpeeds { get; } = new TitledList<CameraMenuItem>(ShutterBinary.Select(p => new CameraMenuItem(p.Key.ToString(), p.Value, "setsetting", "shtrspeed", p.Key + "/256")), "Shutter Speed");

        public static TitledList<CameraMenuItem> Apertures { get; } = new TitledList<CameraMenuItem>(ApertureBinary.Select(p => new CameraMenuItem(p.Key.ToString(), p.Value, "setsetting", "focal", p.Key + "/256")), "Aperture");

        public TitledList<CameraMenuItem> VideoFormat { get; set; }

        public TitledList<CameraMenuItem> VideoQuality { get; set; }

        public TitledList<CameraMenuItem> WhiteBalances { get; set; }

        public TitledList<CameraMenuItem> FlashModes { get; set; }

        public TitledList<CameraMenuItem> AutobracketModes { get; set; }

        public CameraMenuItem SingleShootMode { get; set; }

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

        public static TextBinValue GetIso(ArraySegment<byte> array)
        {
            var bin = toShort(array, 127);
            if (IsoBinary.TryGetValue(bin, out var val))
            {
                return new TextBinValue(val, bin);
            }

            val = FindClosest(IsoBinary, bin);
            return new TextBinValue(val, bin);
        }

        public static TextBinValue GetShutter(ArraySegment<byte> array)
        {
            var bin = toShort(array, 68);
            if (ShutterBinary.TryGetValue(bin, out var val))
            {
                return new TextBinValue(val, bin);
            }

            val = FindClosest(ShutterBinary, bin);
            return new TextBinValue(val, bin);
        }

        public static TextBinValue GetAperture(ArraySegment<byte> array)
        {
            var bin = toShort(array, 56);
            if (ApertureBinary.TryGetValue(bin, out var val))
            {
                return new TextBinValue(val, bin);
            }

            val = FindClosest(ApertureBinary, bin);
            return new TextBinValue(val, bin);
        }

        private static short toShort(ArraySegment<byte> array, int i)
        {
            return (short)((((int)array.Array[array.Offset + i]) << 8) + array.Array[array.Offset + i + 1]);
        }

        public static MenuSet TryParseMenuSet(LumixResponces.RawMenuSet resultMenuSet, string lang)
        {
            var parsers = new AbstractMenuSetParser[]
            {
                new GH4Parser(),
                new GH3Parser()
            };

            var exceptions = new List<Exception>();
            foreach (var p in parsers)
            {
                try
                {
                    return p.ParseMenuSet(resultMenuSet, lang);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException("Could not parse MenuSet", exceptions);
        }
    }
}