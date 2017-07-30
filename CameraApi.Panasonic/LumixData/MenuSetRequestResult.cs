using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class MenuSetRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "menuset")]
        public RawMenuSet MenuSet { get; set; }
    }
}