using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    public enum YesNo
    {
        [XmlEnum(Name = "no")]
        No = 0,

        [XmlEnum(Name = "yes")]
        Yes = 1
    }
}