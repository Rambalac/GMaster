namespace GMaster.Camera
{
    public class MenuSet
    {
        public TitledList<CameraMenuItemText> AutofocusModes { get; set; }

        public TitledList<CameraMenuItemText> BurstModes { get; set; }

        public TitledList<CameraMenuItemText> CreativeControls { get; set; }

        public TitledList<CameraMenuItemText> CustomMultiModes { get; set; }

        public TitledList<CameraMenuItemText> DbValues { get; set; }

        public TitledList<CameraMenuItemText> ExposureShifts { get; set; }

        public TitledList<CameraMenuItemText> IsoValues { get; set; }

        public TitledList<CameraMenuItemText> LiveviewQuality { get; set; }

        public TitledList<CameraMenuItemText> MeteringMode { get; set; }

        public TitledList<CameraMenuItemText> PeakingModes { get; set; }

        public TitledList<CameraMenuItemText> PhotoQuality { get; set; }

        public TitledList<CameraMenuItemText> PhotoSizes { get; set; }

        public TitledList<CameraMenuItemText> PhotoStyles { get; set; }

        public TitledList<CameraMenuItemText> PhotoAspects { get; set; }

        public TitledList<CameraMenuItemText> Angles { get; set; }

        public TitledList<CameraMenuItemText> ShutterSpeeds { get; set; }

        public TitledList<CameraMenuItem256> Apertures { get; set; }

        public TitledList<CameraMenuItemText> VideoFormat { get; set; }

        public TitledList<CameraMenuItemText> VideoQuality { get; set; }

        public TitledList<CameraMenuItemText> WhiteBalances { get; set; }

        public TitledList<CameraMenuItemText> FlashModes { get; set; }

        public TitledList<CameraMenuItemText> AutobracketModes { get; set; }

        public CameraMenuItemText SingleShootMode { get; set; }
    }
}