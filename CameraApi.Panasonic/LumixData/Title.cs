namespace CameraApi.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using GMaster.Core.Tools;

    [XmlRoot(ElementName = "title")]
    public class Title : IStringIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}