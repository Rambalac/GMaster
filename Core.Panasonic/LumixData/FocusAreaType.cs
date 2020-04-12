namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public enum LumixFocusAreaType
    {
        OneAreaSelected = 0x0001,
        FaceOther = 0xff01,
        MainFace = 0x0002,
        Eye = 0xff09,
        TrackUnlock = 0xff03,
        TrackLock = 0x0003,
        MfAssistSelection = 0x0005,
        MfAssistPinP = 0x0006,
        MfAssistFullscreen = 0x0007,
        MfAssistLimit = 0x0008,
        Box = 0xff02,
        Cross = 0xff04
    }
}