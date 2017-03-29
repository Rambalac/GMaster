namespace GMaster.Camera.LumixResponces
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "title")]
    public class Title : IIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}