using System;

namespace GMaster.Camera.LumixResponces
{
    using System.IO;
    using System.Xml.Serialization;

    public interface IIdItem
    {
        string Id { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item : IIdItem
    {
        [XmlAttribute(AttributeName = "cmd_mode")]
        public string CmdMode { get; set; }

        [XmlAttribute(AttributeName = "cmd_type")]
        public string CmdType { get; set; }

        [XmlAttribute(AttributeName = "cmd_value")]
        public string CmdValue { get; set; }

        [XmlAttribute(AttributeName = "cmd_value2")]
        public string CmdValue2 { get; set; }

        [XmlAttribute(AttributeName = "func_type")]
        public string FuncType { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> MenuItems
        {
            get
            {
                return Items;
            }

            set
            {
                if (value != null && value.Count > 0)
                {
                    if (Items != null && Items.Count > 0)
                    {
                        throw new Exception("Items is aready assigned");
                    }

                    Items = value;
                }
            }
        }

        [XmlArray("group")]
        [XmlArrayItem("item")]
        public HashCollection<Item> GroupItems
        {
            get
            {
                return Items;
            }

            set
            {
                if (value != null && value.Count > 0)
                {
                    if (Items != null && Items.Count > 0)
                    {
                        throw new Exception("Items is aready assigned");
                    }

                    Items = value;
                }
            }
        }

        public HashCollection<Item> Items { get; set; }

        [XmlAttribute(AttributeName = "title_id")]
        public string TitleId { get; set; }
    }

    [XmlRoot(ElementName = "language")]
    public class Language : IIdItem
    {
        [XmlAttribute(AttributeName = "default")]
        public YesNo Default { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Id { get; set; }

        [XmlElement(ElementName = "title")]
        public HashCollection<Title> Titles { get; set; }
    }

    public class MenuHolder
    {
        [XmlArray("menu")]
        [XmlArrayItem("item")]
        public HashCollection<Item> Items { get; set; }
    }

    [XmlRoot(ElementName = "menuset")]
    public class MenuSet
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

    [XmlRoot(ElementName = "camrply")]
    public class MenuSetRuquestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "menuset")]
        public MenuSet MenuSet { get; set; }
    }

    [XmlRoot(ElementName = "title")]
    public class Title : IIdItem
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "titlelist")]
    public class TitleList
    {
        [XmlAttribute(AttributeName = "date")]
        public string Date { get; set; }

        [XmlElement(ElementName = "language")]
        public HashCollection<Language> Languages { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }
}
