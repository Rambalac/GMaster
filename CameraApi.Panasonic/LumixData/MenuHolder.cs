using System.Xml.Serialization;
using GMaster.Core.Tools;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}