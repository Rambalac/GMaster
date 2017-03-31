using System.Xml.Serialization;

namespace GMaster.Camera.LumixResponces
{
    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}