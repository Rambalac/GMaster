namespace GMaster.Camera.LumixData
{
    using System.Xml.Serialization;
    using Tools;

    [XmlRoot(ElementName = "language")]
    public class Language : IStringIdItem
    {
        [XmlAttribute(AttributeName = "default")]
        public YesNo Default { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Id { get; set; }

        [XmlElement(ElementName = "title")]
        public HashCollection<Title> Titles { get; set; }
    }
}