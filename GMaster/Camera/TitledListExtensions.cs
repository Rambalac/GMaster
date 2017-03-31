using System.Collections.Generic;
using GMaster.Camera.LumixResponces;

namespace GMaster.Camera
{
    public static class TitledListExtensions
    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
            where TItem : IIdItem
        {
            return new TitledList<TItem>(items, title);
        }
    }
}