namespace GMaster.Camera.LumixResponces
{
    using System.Xml.Serialization;

    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}