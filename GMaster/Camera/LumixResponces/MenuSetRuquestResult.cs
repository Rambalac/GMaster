using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    [XmlRoot(ElementName = "camrply")]
    public class MenuSetRuquestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "menuset")]
        public RawMenuSet MenuSet { get; set; }
    }
}
