namespace GMaster.Camera.LumixData
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "menuset")]
    public class RawMenuSet
    {
        [XmlAttribute(AttributeName = "date")]
        public string Date { get; set; }

        [XmlElement(ElementName = "drivemode")]
        public MenuHolder DriveMode { get; set; }

        [XmlElement(ElementName = "mainmenu")]
        public MenuHolder MainMenu { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }

        [XmlElement(ElementName = "photosettings")]
        public MenuHolder Photosettings { get; set; }

        [XmlElement(ElementName = "qmenu")]
        public MenuHolder Qmenu { get; set; }

        [XmlElement(ElementName = "qmenu2")]
        public MenuHolder Qmenu2 { get; set; }

        [XmlElement(ElementName = "titlelist")]
        public TitleList TitleList { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }
}