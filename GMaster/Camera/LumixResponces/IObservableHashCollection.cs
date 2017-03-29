namespace GMaster.Camera.LumixResponces
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public interface IObservableHashCollection : INotifyPropertyChanged
    {
        void Add(string pairKey, IIdItem pairValue);

        IEnumerable<IIdItem> GetAll();
    }
}