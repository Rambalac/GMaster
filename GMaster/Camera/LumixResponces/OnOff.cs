using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    public enum OnOff
    {
        [XmlEnum(Name = "on")]
        On,

        [XmlEnum(Name = "off")]
        Off
    }
}