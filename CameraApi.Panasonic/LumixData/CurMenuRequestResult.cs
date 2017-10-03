using System.Xml.Serialization;

namesapce CameraApi.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class CurMenuRequestResult : BaseRequestResult
    {
        [XmlElement("menuinfo")]
        public MenuInfo MenuInfo { get; set; }
    }
}