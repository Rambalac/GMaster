using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class FocusModeRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "state")]
        public CameraState State { get; set; }

        [XmlElement("settingvalue")]
        public FocusModeElement Value { get; set; }

        public class FocusModeElement
        {
            [XmlAttribute("focusmode")]
            public FocusMode FocusMode { get; set; }
        }
    }
}