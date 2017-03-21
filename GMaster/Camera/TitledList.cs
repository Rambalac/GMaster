namespace GMaster.Camera
{
    using System.Collections.Generic;

    public class TitledList<TItem> : List<TItem>
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