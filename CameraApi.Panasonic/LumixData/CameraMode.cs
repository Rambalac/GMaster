// ReSharper disable InconsistentNaming

using System;
using GMaster.Core.Tools;

#pragma warning disable SA1300 // Element must begin with upper-case letter

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [Flags]
    public enum CameraModeFlags
    {
        None = 0,
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

        VideoRecording = 05,

        [EnumValue(CameraModeFlags.Video)]
        vP = 0x3c,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Video)]
        vA = 0x3d,

        [EnumValue(CameraModeFlags.Shutter | CameraModeFlags.Video)]
        vS = 0x3e,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Shutter | CameraModeFlags.Video)]
        vM = 0x3f,

        [EnumValue(CameraModeFlags.Aperture | CameraModeFlags.Shutter | CameraModeFlags.Photo)]
        Unknown = 0,

        [EnumValue(CameraModeFlags.None)]
        MFAssist = 0xff
    }
}

#pragma warning restore SA1300 // Element must begin with upper-case letter