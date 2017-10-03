using System.Xml.Serialization;

namespace CameraApi.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class BaseRequestResult
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
    }
}