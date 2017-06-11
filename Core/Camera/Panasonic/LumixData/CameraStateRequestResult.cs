using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class CameraStateRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "state")]
        public CameraState State { get; set; }
    }
}