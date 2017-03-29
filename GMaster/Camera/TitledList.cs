namespace GMaster.Camera
{
    using System.Collections.Generic;
    using LumixResponces;

    public class TitledList<TItem> : HashCollection<TItem>
        where TItem : IIdItem
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