using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public enum SdMemorySet
    {
        [XmlEnum(Name = "set")]
        Set = 1,

        [XmlEnum(Name = "unset")]
        Unset = 0
    }
}