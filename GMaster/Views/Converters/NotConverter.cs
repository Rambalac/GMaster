namespace GMaster.Views.Converters
{
    using System;
    using Windows.UI.Xaml.Data;

    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value as bool? ?? false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}