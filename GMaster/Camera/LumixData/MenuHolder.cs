namespace GMaster.Camera.LumixData
{
    using System.Xml.Serialization;
    using Tools;

    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}