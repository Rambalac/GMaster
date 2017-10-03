using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    public enum SdMemorySet
    {
        [XmlEnum(Name = "set")]
        Set = 1,

        [XmlEnum(Name = "unset")]
        Unset = 0
    }
}