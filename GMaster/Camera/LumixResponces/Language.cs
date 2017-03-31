using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    [XmlRoot(ElementName = "language")]
    public class Language : IIdItem
    {
        [XmlAttribute(AttributeName = "default")]
        public YesNo Default { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Id { get; set; }

        [XmlElement(ElementName = "title")]
        public HashCollection<Title> Titles { get; set; }
    }
}