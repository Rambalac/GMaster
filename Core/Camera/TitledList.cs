namespace GMaster.Core.Camera
{
    using System.Collections.Generic;
    using Tools;

    public class TitledList<TItem> : HashCollection<TItem>
        where TItem : IStringIdItem
    {
        public TitledList()
        {
        }

        public TitledList(IEnumerable<TItem> collection, string title)
            : base(collection)
        {
            Title = title;
        }

        public string Title { get; }
    }
}