namespace GMaster.Camera.LumixResponces
{
    using System.Xml.Serialization;

    public enum OnOff
    {
        [XmlEnum(Name = "on")]
        On,

        [XmlEnum(Name = "off")]
        Off
    }
}