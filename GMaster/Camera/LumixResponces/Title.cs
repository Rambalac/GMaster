using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    [XmlRoot(ElementName = "title")]
    public class Title : IIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}