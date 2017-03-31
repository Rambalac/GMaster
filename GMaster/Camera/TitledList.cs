using System.Collections.Generic;
using GMaster.Camera.LumixResponces;

namespace GMaster.Camera
{
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