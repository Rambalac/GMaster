namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using LumixData;
    using Tools;

    public abstract class CameraParser
    {
        public IReadOnlyDictionary<int, string> ApertureBinary { get; } = new Dictionary<int, string>
        {
            { 392, "1.7" },
            { 427, "1.8" },
            { 512, "2.0" },
            { 598, "2.2" },
            { 683, "2.5" },
            { 768, "2.8" },
            { 854, "3.2" },
            { 938, "3.5" },
            { 1024, "4.0" },
            { 1110, "4.5" },
            { 1195, "5.0" },
            { 1280, "5.6" },
            { 1366, "6.3" },
            { 1451, "7.1" },
            { 1536, "8" },
            { 1622, "9" },
            { 1707, "10" },
            { 1792, "11" },
            { 1878, "13" },
            { 1963, "14" },
            { 2048, "16" },
            { 2134, "18" },
            { 2219, "20" },
            { 2304, "22" },
        };

        public HashCollection<Title> CurrentLanguage { get; set; }

        public HashCollection<Title> DefaultLanguage { get; set; }

        public abstract IReadOnlyDictionary<int, string> IsoBinary { get; }

        // public IReadOnlyDictionary<int, string> OpenedApertureBinary { get; } = new Dictionary<int, string>
        // {
        //    { 1024, "4" },
        //    { 1042, "4.1" },
        //    { 1060, "4.2" },
        //    { 1077, "4.3" },
        //    { 1094, "4.4" },
        //    { 1110, "4.5" },
        //    { 1111, "4.5" },
        //    { 1143, "4.7" },
        //    { 1189, "5" },
        //    { 1195, "5" },
        //    { 1232, "5.3" },
        //    { 1259, "5.5" },
        //    { 1273, "5.6" },
        //    { 1280, "5.6" },
        //    { 1286, "5.7" },
        //    { 1298, "5.8" }
        // };
        public IReadOnlyDictionary<int, string> ShutterBinary { get; } = new Dictionary<int, string>
        {
            { 3584, "16000" },
            { 3499, "13000" },
            { 3414, "10000" },
            { 3328, "8000" },
            { 3243, "6400" },
            { 3158, "5000" },
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

        public static string ApertureBinToText(int bin)
        {
            return Math.Pow(2, bin / 512.0).ToString("F1", CultureInfo.InvariantCulture);
        }

        public static CameraParser TryParseMenuSet(RawMenuSet resultMenuSet, string lang, out MenuSet menuset)
        {
            var parsers = new CameraParser[]
            {
                new GH4Parser(),
                new GH3Parser()
            };

            var exceptions = new List<Exception>();
            foreach (var p in parsers)
            {
                try
                {
                    var ms = p.ParseMenuSet(resultMenuSet, lang);
                    if (ms == null)
                    {
                        continue;
                    }

                    menuset = ms;
                    return p;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException("Could not parse MenuSet", exceptions);
        }

        public string GetText(string id)
        {
            if (CurrentLanguage != null && CurrentLanguage.TryGetValue(id, out var text))
            {
                return text.Text;
            }

            if (DefaultLanguage.TryGetValue(id, out var text2))
            {
                return text2.Text;
            }

            throw new KeyNotFoundException("Title not found: " + id);
        }

        public LensInfo ParseLensInfo(string raw)
        {
            Debug.WriteLine(raw);
            var values = raw.Split(',');
            return new LensInfo
            {
                MaxZoom = int.TryParse(values[7], out var res1) ? res1 : 0,
                MinZoom = int.TryParse(values[8], out var res2) ? res2 : 0,
                OpenedAperture = int.TryParse(values[2].Replace("/256", string.Empty), out var res3) ? res3 : 0,
                ClosedAperture = int.TryParse(values[1].Replace("/256", string.Empty), out var res4) ? res4 : int.MaxValue,
                HasPowerZoom = values[6] == "on"
            };
        }

        public FocusPosition ParseFocus(string focus)
        {
            var values = focus.Split(',');
            if (values.Length != 3)
            {
                throw new Exception("Strange focus values: " + focus);
            }

            if (values[0] != "ok" || !int.TryParse(values[2], out var max) || !int.TryParse(values[1], out var val))
            {
                Debug.WriteLine("Cannont parse focus: " + focus);
                return null;
            }

            return new FocusPosition { Value = max - val, Maximum = max };
        }

        public virtual MenuSet ParseMenuSet(RawMenuSet menuset, string lang)
        {
            var result = new MenuSet
            {
                ShutterSpeeds = new TitledList<CameraMenuItemText>(ShutterBinary.Select(p => new CameraMenuItemText(p.Key.ToString(), p.Value, "setsetting", "shtrspeed", p.Key + "/256")), "Shutter Speed"),
                Apertures = new TitledList<CameraMenuItem256>(ApertureBinary.Select(p => new CameraMenuItem256(p.Key.ToString(), p.Value, "setsetting", "focal", p.Key)), "Aperture")
            };

            return InnerParseMenuSet(result, menuset, lang) ? result : null;
        }

        protected abstract bool InnerParseMenuSet(MenuSet result, RawMenuSet menuset, string lang);

        protected CameraMenuItemText ToMenuItem(Item item)
        {
            return new CameraMenuItemText(item, GetText(item.TitleId));
        }

        protected TitledList<CameraMenuItemText> ToMenuItems(Item menuitem)
        {
            try
            {
                return menuitem.Items
                    .Select(i => new CameraMenuItemText(i, GetText(i.TitleId)))
                    .ToTitledList(GetText(menuitem.TitleId));
            }
            catch (KeyNotFoundException)
            {
                return new TitledList<CameraMenuItemText>();
            }
        }
    }
}
