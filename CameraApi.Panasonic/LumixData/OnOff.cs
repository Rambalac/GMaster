using System.Xml.Serialization;
using GMaster.Core.Tools;

namesapce CameraApi.Panasonic.LumixData
{
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