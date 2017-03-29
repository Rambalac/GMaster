namespace GMaster.Camera
{
    using System.Collections.Generic;
    using System.Linq;
    using LumixResponces;

    public abstract class AbstractMenuSetParser
    {
        public HashCollection<Title> CurrentLanguage { get; set; }

        public HashCollection<Title> DefaultLanguage { get; set; }

        public abstract MenuSet ParseMenuSet(RawMenuSet menuset, string lang);

        public string GetText(string id)
        {
            if (CurrentLanguage != null && CurrentLanguage.TryGetValue(id, out var text))
            {
                return text.Text;
            }

            if (DefaultLanguage.TryGetValue(id, out var text2))
            {
                return text2.Text;
            }

            throw new KeyNotFoundException("Title not found: " + id);
        }

        protected CameraMenuItem ToMenuItem(Item item)
        {
            return new CameraMenuItem(item, GetText(item.TitleId));
        }

        protected TitledList<CameraMenuItem> ToMenuItems(Item menuitem)
        {
            try
            {
                return menuitem.Items
                    .Select(i => new CameraMenuItem(i, GetText(i.TitleId)))
                    .ToTitledList(GetText(menuitem.TitleId));
            }
            catch (KeyNotFoundException)
            {
                return new TitledList<CameraMenuItem>();
            }
        }
    }
}
