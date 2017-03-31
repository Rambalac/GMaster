namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LumixResponces;

    public abstract class CameraParser
    {
        public abstract IReadOnlyDictionary<int, string> IsoBinary { get; }

        public IReadOnlyDictionary<int, string> ApertureBinary { get; } = new Dictionary<int, string>
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

        public IReadOnlyDictionary<int, string> ShutterBinary { get; } = new Dictionary<int, string>
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

        public HashCollection<Title> CurrentLanguage { get; set; }

        public HashCollection<Title> DefaultLanguage { get; set; }

        public virtual MenuSet ParseMenuSet(RawMenuSet menuset, string lang)
        {
            var result = new MenuSet
            {
                ShutterSpeeds = new TitledList<CameraMenuItem>(ShutterBinary.Select(p => new CameraMenuItem(p.Key.ToString(), p.Value, "setsetting", "shtrspeed", p.Key + "/256")), "Shutter Speed"),
                Apertures = new TitledList<CameraMenuItem>(ApertureBinary.Select(p => new CameraMenuItem(p.Key.ToString(), p.Value, "setsetting", "focal", p.Key + "/256")), "Aperture")
            };

            InnerParseMenuSet(result, menuset, lang);
            return result;
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
                    menuset = p.ParseMenuSet(resultMenuSet, lang);
                    return p;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException("Could not parse MenuSet", exceptions);
        }

        protected abstract void InnerParseMenuSet(MenuSet result, RawMenuSet menuset, string lang);

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

        protected CameraMenuItem ToMenuItem(Item item)
        {
            return new CameraMenuItem(item, GetText(item.TitleId));
        }

        protected TitledList<CameraMenuItem> ToMenuItems(Item menuitem)
        {
            try
            {
                return menuitem.Items
                    .Select(i => new CameraMenuItem(i, GetText(i.TitleId)))
                    .ToTitledList(GetText(menuitem.TitleId));
            }
            catch (KeyNotFoundException)
            {
                return new TitledList<CameraMenuItem>();
            }
        }
    }
}
