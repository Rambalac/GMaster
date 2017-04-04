namespace Tools
{
    using System.Collections;
    using System.Collections.Generic;

    public class HashCollection<TItem> : ICollection<TItem>
        where TItem : IStringIdItem
    {
        private readonly Dictionary<string, TItem> dictionary = new Dictionary<string, TItem>();

        public HashCollection()
        {
        }

        public HashCollection(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                dictionary.Add(item.Id, item);
            }
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public TItem this[string index]
        {
            get
            {
                return dictionary.TryGetValue(index, out var value) ? value : throw new KeyNotFoundException("Key not found: " + index);
            }
        }

        public virtual void Add(TItem item)
        {
            dictionary.Add(item.Id, item);
        }

        public virtual void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(TItem item)
        {
            return dictionary.ContainsKey(item.Id);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            dictionary.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        public virtual bool Remove(TItem item)
        {
            return dictionary.Remove(item.Id);
        }

        public bool TryGetValue(string id, out TItem value)
        {
            return dictionary.TryGetValue(id, out value);
        }
    }
}