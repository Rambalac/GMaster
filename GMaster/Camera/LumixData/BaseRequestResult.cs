namespace GMaster.Camera.LumixData
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "camrply")]
    public class BaseRequestResult
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
    }
}