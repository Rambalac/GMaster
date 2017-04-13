namespace Tools
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using GMaster.Core.Tools;

    public interface IObservableHashCollection : INotifyCollectionChanged, INotifyPropertyChanged
    {
        void Add(string pairKey, IStringIdItem pairValue);

        IEnumerable<IStringIdItem> GetAll();
    }
}