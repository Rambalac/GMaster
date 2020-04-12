namespace CameraApi.Panasonic
{
    using System.Collections.Generic;
    using GMaster.Core.Tools;

    public static class TitledListExtensions
    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
            where TItem : ICameraMenuItem
        {
            return new TitledList<TItem>(items, title);
        }
    }
}