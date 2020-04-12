// ReSharper disable InconsistentNaming

#pragma warning disable SA1300 // Element must begin with upper-case letter

namespace CameraApi.Panasonic.LumixData
{
    using System;
    using GMaster.Core.Tools;

    [Flags]
    public enum LumixCameraModeFlags
    {
        None = 0,
        Shutter = 1 << 0,
        Aperture = 1 << 1,
        Video = 1 << 2,
        Photo = 1 << 3
    }

    public enum LumixCameraMode
    {
        [EnumValue(LumixCameraModeFlags.Photo)]
        iA = 09,

        [EnumValue(LumixCameraModeFlags.Photo)]
        P = 01,

        [EnumValue(LumixCameraModeFlags.Aperture | LumixCameraModeFlags.Photo)]
        A = 02,

        [EnumValue(LumixCameraModeFlags.Shutter | LumixCameraModeFlags.Photo)]
        S = 03,

        [EnumValue(LumixCameraModeFlags.Aperture | LumixCameraModeFlags.Shutter | LumixCameraModeFlags.Photo)]
        M = 04,

        VideoRecording = 05,

        [EnumValue(LumixCameraModeFlags.Video)]
        vP = 0x3c,

        [EnumValue(LumixCameraModeFlags.Aperture | LumixCameraModeFlags.Video)]
        vA = 0x3d,

        [EnumValue(LumixCameraModeFlags.Shutter | LumixCameraModeFlags.Video)]
        vS = 0x3e,

        [EnumValue(LumixCameraModeFlags.Aperture | LumixCameraModeFlags.Shutter | LumixCameraModeFlags.Video)]
        vM = 0x3f,

        [EnumValue(LumixCameraModeFlags.Aperture | LumixCameraModeFlags.Shutter | LumixCameraModeFlags.Photo)]
        Unknown = 0,

        [EnumValue(LumixCameraModeFlags.None)]
        MFAssist = 0xff
    }
}

#pragma warning restore SA1300 // Element must begin with upper-case letter