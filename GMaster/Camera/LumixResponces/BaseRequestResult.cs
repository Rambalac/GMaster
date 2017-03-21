using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    [XmlRoot(ElementName = "camrply")]
    public class BaseRequestResult
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
    }
}