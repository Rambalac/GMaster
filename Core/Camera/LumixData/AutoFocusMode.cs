namespace GMaster.Core.Camera.LumixData
{
    using Tools;

    public enum AutoFocusModeFlags
    {
        None,
        TouchAFRelease
    }

    public enum AutoFocusMode
    {
        [EnumValue(AutoFocusModeFlags.None)]
        Unknown = -1,

        [EnumValue(AutoFocusModeFlags.None)]
        Manual = 0,

        [EnumException(4)]
        [EnumValue(AutoFocusModeFlags.TouchAFRelease)]
        Face = 3,

        [EnumValue(AutoFocusModeFlags.TouchAFRelease)]
        Track = 5,

        [EnumException(6)]
        [EnumValue(AutoFocusModeFlags.TouchAFRelease)]
        MultiArea = 8,

        [EnumValue(AutoFocusModeFlags.TouchAFRelease)]
        FreeMultiArea = 11,

        [EnumValue(AutoFocusModeFlags.None)]
        OneArea = 1,

        [EnumValue(AutoFocusModeFlags.None)]
        Pinpoint = 7
    }
}