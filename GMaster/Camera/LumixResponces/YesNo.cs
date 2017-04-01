namespace GMaster.Camera.LumixResponces
{
    using System.Xml.Serialization;

    public enum YesNo
    {
        [XmlEnum(Name = "no")]
        No = 0,
        [XmlEnum(Name = "yes")]
        Yes = 1
    }
}