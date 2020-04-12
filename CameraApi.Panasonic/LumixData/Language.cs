namespace CameraApi.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using GMaster.Core.Tools;

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