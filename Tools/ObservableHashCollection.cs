namespace GMaster.Tools
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using Core.Tools;

    public class ObservableHashCollection<TItem> : HashCollection<TItem>, IObservableHashCollection
        where TItem : IStringIdItem
    {
        public ObservableHashCollection(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                base.Add(item);
            }
        }

        public ObservableHashCollection()
        {
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public override void Add(TItem item)
        {
            base.Add(item);
            if (item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Item_PropertyChanged;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged();
        }

        public void Add(string pairKey, IStringIdItem item)
        {
            Add((TItem)item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged();
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                base.Add(item);
                if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += Item_PropertyChanged;
                }
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
            OnPropertyChanged();
        }

        public override void Clear()
        {
            var all = this.ToList();
            base.Clear();
            foreach (var item in all)
            {
                if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= Item_PropertyChanged;
                }
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, all));
            OnPropertyChanged();
        }

        public IEnumerable<IStringIdItem> GetAll()
        {
            return this.Cast<IStringIdItem>();
        }

        public override bool Remove(TItem item)
        {
            if (TryGetValue(item.Id, out _))
            {
                base.Remove(item);
                if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= Item_PropertyChanged;
                }

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                OnPropertyChanged();
                return true;
            }

            return false;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender));
            OnPropertyChanged();
        }

        private void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}