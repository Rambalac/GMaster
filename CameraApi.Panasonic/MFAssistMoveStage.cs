using GMaster.Core.Tools;

namespace CameraApi.Panasonic
{
    public enum PinchStage
    {
        [EnumValue("start")]
        Start = 0,

        [EnumValue("stop")]
        Stop = 1,

        [EnumValue("continue")]
        Continue = 2,

        Single = 3
    }
}