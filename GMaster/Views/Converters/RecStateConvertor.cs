namespace GMaster.Views.Converters
{
    using System;
    using Camera;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class RecStateConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as RecState? ?? RecState.Stopped) != RecState.Started ? "\uE20A" : "\uE25D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}