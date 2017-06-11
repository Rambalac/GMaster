using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public enum YesNo
    {
        [XmlEnum(Name = "no")]
        No = 0,

        [XmlEnum(Name = "yes")]
        Yes = 1
    }
}