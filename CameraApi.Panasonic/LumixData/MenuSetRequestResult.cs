using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class MenuSetRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "menuset")]
        public RawMenuSet MenuSet { get; set; }
    }
}