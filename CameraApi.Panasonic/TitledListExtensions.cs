using System.Collections.Generic;
using GMaster.Core.Tools;

namespace CameraApi.Panasonic
{
    public static class TitledListExtensions
    {
        public static TitledList<TItem> ToTitledList<TItem>(this IEnumerable<TItem> items, string title)
            where TItem : IStringIdItem
        {
            return new TitledList<TItem>(items, title);
        }
    }
}