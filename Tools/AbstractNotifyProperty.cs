namespace Tools
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;

    public abstract class AbstractNotifyProperty : INotifyPropertyChanged
    {

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}