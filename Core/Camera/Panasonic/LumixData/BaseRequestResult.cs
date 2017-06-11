using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class BaseRequestResult
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
    }
}