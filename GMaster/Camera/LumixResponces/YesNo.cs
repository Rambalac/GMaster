using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    public enum YesNo
    {
        [XmlEnum(Name = "no")]
        No = 0,
        [XmlEnum(Name = "yes")]
        Yes = 1
    }
}