using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GMaster.Annotations;

namespace GMaster.Camera.LumixResponces
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IObservableHashCollection
    {
        IEnumerable<IIdItem> GetAll();
        void Add(string pairKey, IIdItem pairValue);
    }

    public class ObservableHashCollection<TItem> : HashCollection<TItem>, IObservableHashCollection where TItem : IIdItem
    {
        public override void Add(TItem item)
        {
            base.Add(item);
            ItemAdded?.Invoke(this, item);
        }

        public override void Clear()
        {
            var all = this.ToList();
            base.Clear();
            foreach (var item in all)
            {
                ItemRemoved(this, item);
            }
        }

        public override bool Remove(TItem item)
        {
            if (TryGetValue(item.Id, out var value))
            {
                base.Remove(item);
                ItemRemoved(this, value);
                return true;
            }

            return false;
        }

        public event Action<ObservableHashCollection<TItem>, TItem> ItemAdded;
        public event Action<ObservableHashCollection<TItem>, TItem> ItemRemoved;

        public IEnumerable<IIdItem> GetAll()
        {
            return this.Cast<IIdItem>();
        }

        public void Add(string pairKey, IIdItem pairValue)
        {
            Add((TItem)pairValue);
        }
    }

    public class HashCollection<TItem> : ICollection<TItem>
        where TItem : IIdItem
    {
        private readonly Dictionary<string, TItem> dictionary = new Dictionary<string, TItem>();

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public TItem this[string index]
        {
            get
            {
                return dictionary.TryGetValue(index, out var value) ? value : throw new KeyNotFoundException("Key not found: " + index);
            }
        }

        public bool TryGetValue(string id, out TItem value)
        {
            return dictionary.TryGetValue(id, out value);
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
    }
}