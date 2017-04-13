// ReSharper disable InconsistentNaming

#pragma warning disable SA1300 // Element must begin with upper-case letter

namespace GMaster.Core.Camera
{
    using System;
    using Tools;

    [Flags]
    public enum CameraModeFlags
    {
        Shutter = 1 << 0,
        Aperture = 1 << 1,
        Video = 1 << 2,
        Photo = 1 << 3
    }

    public enum CameraMode
    {
        [EnumValue(CameraModeFlags.Photo)]
        iA = 09,

        [EnumValue(CameraModeFlags.Photo)]
        P = 01,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Photo)]
        A = 02,

        [EnumValue(CameraModeFlags.Shutter | CameraModeFlags.Photo)]
        S = 03,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Shutter | CameraModeFlags.Photo)]
        M = 04,

        [EnumValue(CameraModeFlags.Video)]
        vP = 0x3c,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Video)]
        vA = 0x3d,

        [EnumValue(CameraModeFlags.Shutter | CameraModeFlags.Video)]
        vS = 0x3e,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Shutter | CameraModeFlags.Video)]
        vM = 0x3f,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Shutter | CameraModeFlags.Photo)]
        Unknown = 0
    }
}

#pragma warning restore SA1300 // Element must begin with upper-case letter