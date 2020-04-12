namespace GMaster.Core.Camera.Panasonic.LumixData
{
    using System.Xml.Serialization;

    public enum SdMemorySet
    {
        [XmlEnum(Name = "set")]
        Set = 1,

        [XmlEnum(Name = "unset")]
        Unset = 0
    }
}