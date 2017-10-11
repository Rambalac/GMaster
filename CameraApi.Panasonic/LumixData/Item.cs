namespace CameraApi.Panasonic.LumixData
{
    using System;
    using System.Xml.Serialization;
    using GMaster.Core.Tools;

    [XmlRoot(ElementName = "item")]
    public class Item : IStringIdItem
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

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        public HashCollection<Item> Items { get; set; }

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

        [XmlAttribute(AttributeName = "title_id")]
        public string TitleId { get; set; }
    }
}