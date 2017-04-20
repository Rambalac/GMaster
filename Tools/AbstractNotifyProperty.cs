namespace GMaster.Tools
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}