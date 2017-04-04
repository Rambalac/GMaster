namespace Tools
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Annotations;

    public class ObservableHashCollection<TItem> : HashCollection<TItem>, IObservableHashCollection
        where TItem : IStringIdItem
    {
        public event Action<ObservableHashCollection<TItem>, TItem> ItemAdded;

        public event Action<ObservableHashCollection<TItem>, TItem> ItemRemoved;

        public event PropertyChangedEventHandler PropertyChanged;

        public override void Add(TItem item)
        {
            base.Add(item);
            ItemAdded?.Invoke(this, item);
            if (item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Settings_PropertyChanged;
            }

            OnPropertyChanged(null);
        }

        public void Add(string pairKey, IStringIdItem pairValue)
        {
            Add((TItem)pairValue);
        }

        public override void Clear()
        {
            var all = this.ToList();
            base.Clear();
            foreach (var item in all)
            {
                ItemRemoved?.Invoke(this, item);
                if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= Settings_PropertyChanged;
                }
            }

            OnPropertyChanged(null);
        }

        public IEnumerable<IStringIdItem> GetAll()
        {
            return this.Cast<IStringIdItem>();
        }

        public override bool Remove(TItem item)
        {
            if (TryGetValue(item.Id, out var value))
            {
                base.Remove(item);
                ItemRemoved?.Invoke(this, value);
                if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= Settings_PropertyChanged;
                }

                OnPropertyChanged(null);
                return true;
            }

            return false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(null);
        }
    }
}