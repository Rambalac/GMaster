namespace GMaster.Core.Camera.LumixData
{
    using System.Xml.Serialization;

    public enum SdMemorySet
    {
        [XmlEnum(Name = "set")]
        Set,

        [XmlEnum(Name = "unset")]
        Unset
    }
}