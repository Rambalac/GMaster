namespace CameraApi.Panasonic.LumixData
{
    using System.Xml.Serialization;
    using GMaster.Core.Tools;

    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }
}