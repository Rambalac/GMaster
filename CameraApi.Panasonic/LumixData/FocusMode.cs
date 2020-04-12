// ReSharper disable InconsistentNaming

namespace CameraApi.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using CameraApi.Core;
    using GMaster.Core.Tools;

    public enum LumixFocusMode
    {
        [EnumException(0x5)]
        [XmlEnum(Name = "mf")]
        MF = 0xff,

        [XmlEnum(Name = "afc")]
        AFC = 0x3,

        [XmlEnum(Name = "aff")]
        AFF = 0x2,

        [XmlEnum(Name = "afs")]
        AFS = 0x1,

        Unknown = 0
    }
}