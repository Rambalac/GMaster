namespace GMaster.Core.Camera.LumixData
{
    using System.Xml.Serialization;

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