using System.Xml.Serialization;
using GMaster.Core.Tools;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    [XmlRoot(ElementName = "title")]
    public class Title : IStringIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}