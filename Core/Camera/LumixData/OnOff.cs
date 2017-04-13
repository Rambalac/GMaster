namespace GMaster.Core.Camera.LumixData
{
    using System.Xml.Serialization;
    using Tools;

    public enum OnOff
    {
        [EnumValue("on")]
        [XmlEnum(Name = "on")]
        On,

        [EnumValue("off")]
        [XmlEnum(Name = "off")]
        Off
    }
}