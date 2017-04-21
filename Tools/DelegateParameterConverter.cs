namespace GMaster.Tools
{
    using System;
    using System.Globalization;
    using Windows.UI.Xaml.Data;

    public abstract class DelegateParameterConverter<TFrom, TParam, TTo> : IValueConverter
        where TParam : IConvertible
    {
        protected abstract Func<TFrom, TParam, TTo> Converter { get; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Converter((TFrom)value, (TParam)((IConvertible)parameter).ToType(typeof(TParam), CultureInfo.InvariantCulture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}