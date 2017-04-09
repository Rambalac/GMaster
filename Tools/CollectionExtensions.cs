using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Tools
{
    using System.Collections.Generic;

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this IList<T> col, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                col.Add(item);
            }
        }

        public static void Reset<T>(this IList<T> col, IEnumerable<T> items)
        {
            col.Clear();
            AddRange(col, items);
        }

        public static void AddRange<T>(this IProducerConsumerCollection<T> col, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                col.TryAdd(item);
            }
        }

        public static void Clear<T>(this IProducerConsumerCollection<T> col)
        {
            while (col.TryTake(out _))
            {
            }
        }

        public static void Reset<T>(this IProducerConsumerCollection<T> col, IEnumerable<T> items)
        {
            col.Clear();
            AddRange(col, items);
        }
    }
}