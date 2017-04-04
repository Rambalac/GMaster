namespace Tools
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public interface IObservableHashCollection : INotifyPropertyChanged
    {
        void Add(string pairKey, IStringIdItem pairValue);

        IEnumerable<IStringIdItem> GetAll();
    }
}