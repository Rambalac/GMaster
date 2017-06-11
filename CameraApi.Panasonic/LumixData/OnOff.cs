using System.Xml.Serialization;
using GMaster.Core.Tools;

namespace GMaster.Core.Camera.Panasonic.LumixData
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