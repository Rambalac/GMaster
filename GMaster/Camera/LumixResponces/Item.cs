namespace GMaster.Camera.LumixResponces
{
    using System;
    using System.Xml.Serialization;

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
}