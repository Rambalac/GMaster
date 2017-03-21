using System;

namespace GMaster.Camera
{
    using System.Collections.Generic;
    using System.Linq;
    using LumixResponces;

    public abstract class AbstractMenuSetParser
    {
        public TitledList<CameraMenuItem> AutofocusModes { get; protected set; }

        public TitledList<CameraMenuItem> BurstModes { get; protected set; }

        public TitledList<CameraMenuItem> CreativeControls { get; protected set; }

        public TitledList<CameraMenuItem> CustomMultiModes { get; protected set; }

        public TitledList<CameraMenuItem> DbValues { get; protected set; }

        public TitledList<CameraMenuItem> ExposureShifts { get; protected set; }

        public TitledList<CameraMenuItem> IsoValues { get; protected set; }

        public TitledList<CameraMenuItem> LiveviewQuality { get; protected set; }

        public TitledList<CameraMenuItem> MeteringMode { get; protected set; }

        public TitledList<CameraMenuItem> PeakingModes { get; protected set; }

        public TitledList<CameraMenuItem> PhotoQuality { get; protected set; }

        public TitledList<CameraMenuItem> PhotoSizes { get; protected set; }

        public TitledList<CameraMenuItem> PhotoStyles { get; protected set; }

        public TitledList<CameraMenuItem> PhotoAspects { get; protected set; }

        public TitledList<CameraMenuItem> ShutterSpeeds { get; protected set; }

        public TitledList<CameraMenuItem> VideoFormat { get; protected set; }

        public TitledList<CameraMenuItem> VideoQuality { get; protected set; }

        public TitledList<CameraMenuItem> WhiteBalances { get; protected set; }

        public TitledList<CameraMenuItem> FlashModes { get; protected set; }

        public TitledList<CameraMenuItem> AutobracketModes { get; protected set; }

        public CameraMenuItem SingleShootMode { get; set; }

        protected HashCollection<Title> CurrentLanguage { get; set; }

        protected HashCollection<Title> DefaultLanguage { get; set; }

        public static AbstractMenuSetParser TryParse(MenuSet resultMenuSet, string lang)
        {
            var parsers = new AbstractMenuSetParser[]
            {
                new MenuSetParserGh4(),
                new MenuSetParserGh3()
            };

            var exceptions = new List<Exception>();
            foreach (var p in parsers)
            {
                try
                {
                    if (p.InternalTryParse(resultMenuSet, lang))
                    {
                        return p;
                    }
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

        protected abstract bool InternalTryParse(MenuSet menuset, string lang);

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