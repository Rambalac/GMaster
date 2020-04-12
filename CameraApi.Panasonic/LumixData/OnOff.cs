namespace CameraApi.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using GMaster.Core.Tools;

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