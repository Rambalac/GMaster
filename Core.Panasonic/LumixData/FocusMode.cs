// ReSharper disable InconsistentNaming

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using Tools;

    public enum FocusMode
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