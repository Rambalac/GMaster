namespace GMaster.Core.Camera
{
    public enum FocusAreaType
    {
        OneAreaSelected = 0x0001,
        FaceOther = 0xff01,
        MainFace = 0x0002,
        Eye = 0xff09,
        TrackUnlock = 0xff03,
        TrackLock = 0x0003,

        Box = 0xff02,
        Cross = 0xff04
    }
}