namespace GMaster.Camera
{
    using System.Collections.Generic;
    using LumixResponces;

    public static class TitledListExtensions
    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
            where TItem : IIdItem
        {
            return new TitledList<TItem>(items, title);
        }
    }
}