namespace GMaster.Camera
{
    using System.Collections.Generic;

    public static class TitledListExtensions

    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
        {
            return new TitledList<TItem>(items, title);
        }

    }
}