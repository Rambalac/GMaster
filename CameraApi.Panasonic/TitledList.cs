namespace CameraApi.Panasonic
{
    using System.Collections.Generic;
    using GMaster.Core.Tools;

    public class TitledList<TItem> : HashCollection<TItem>
        where TItem : ICameraMenuItem
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