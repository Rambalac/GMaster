namespace GMaster.Core.Camera.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using Tools;

    [XmlRoot(ElementName = "item")]
    public class CurMenuItem : IStringIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "enabled")]
        public YesNo Enable { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "option")]
        public string Option { get; set; }

        [XmlAttribute(AttributeName = "option2")]
        public string Option2 { get; set; }
    }
}