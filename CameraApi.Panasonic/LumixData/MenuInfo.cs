using System.Xml.Serialization;
using GMaster.Core.Tools;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public class MenuInfo
    {
        [XmlArray("mainmenu")]
        [XmlArrayItem("item")]
        public HashCollection<CurMenuItem> MainMenu { get; set; }

        [XmlArray("photosettings")]
        [XmlArrayItem("item")]
        public HashCollection<CurMenuItem> Photosettings { get; set; }

        [XmlArray("qmenu2")]
        [XmlArrayItem("item")]
        public HashCollection<CurMenuItem> Qmenu2 { get; set; }

        [XmlArray("qmenu")]
        [XmlArrayItem("item")]
        public HashCollection<CurMenuItem> Qmenu { get; set; }
    }
}