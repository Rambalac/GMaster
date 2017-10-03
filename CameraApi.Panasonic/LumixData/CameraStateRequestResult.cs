using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class CameraStateRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "state")]
        public CameraState State { get; set; }
    }
}