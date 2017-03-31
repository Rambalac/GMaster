using System.Collections.Generic;
using System.ComponentModel;

namespace GMaster.Camera.LumixResponces
{
    public interface IObservableHashCollection : INotifyPropertyChanged
    {
        void Add(string pairKey, IIdItem pairValue);

        IEnumerable<IIdItem> GetAll();
    }
}