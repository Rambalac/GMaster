using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [XmlRoot(ElementName = "camrply")]
    public class CurMenuRequestResult : BaseRequestResult
    {
        [XmlElement("menuinfo")]
        public MenuInfo MenuInfo { get; set; }
    }
}