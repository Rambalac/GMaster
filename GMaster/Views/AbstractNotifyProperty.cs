using System.ComponentModel;
using System.Runtime.CompilerServices;
using GMaster.Annotations;

namespace GMaster.Views
{
    public abstract class AbstractNotifyProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected object InnerValue { get; set; }

        public object GetValue()
        {
            return InnerValue;
        }

        public virtual void SetValue(object val)
        {
            if (Equals(val, InnerValue))
            {
                return;
            }

            InnerValue = val;
            OnPropertyChanged();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}