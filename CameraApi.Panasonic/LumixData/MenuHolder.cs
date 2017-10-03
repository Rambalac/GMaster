using System.Xml.Serialization;
using GMaster.Core.Tools;

namesapce CameraApi.Panasonic.LumixData
{
    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}