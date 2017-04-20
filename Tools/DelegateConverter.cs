namespace GMaster.Tools
{
    using System;
    using Windows.UI.Xaml.Data;

    public abstract class DelegateConverter<TFrom, TTo> : IValueConverter
    {
        protected abstract Func<TFrom, TTo> Converter { get; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Converter((TFrom)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}