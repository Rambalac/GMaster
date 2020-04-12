namespace GMaster.Core.Camera.Panasonic
{
    using System.Collections.Generic;
    using Tools;

    public static class TitledListExtensions
    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
            where TItem : IStringIdItem
        {
            return new TitledList<TItem>(items, title);
        }
    }
}