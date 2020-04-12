namespace CameraApi.Core
{
    /// <summary>
    /// Camera AutoFocus mode
    /// </summary>
    public enum AutoFocusMode
    {
        Unknown,
        Manual,
        Face,
        Track,
        MultiArea,
        FreeMultiArea,
        OneArea,
        Pinpoint
    }

    public enum FocusMode
    {
        MF,
        AFC,
        AFF,
        AFS,
        Unknown
    }
}
