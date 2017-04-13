namespace GMaster.Views.Converters
{
    using System;
    using Core.Camera;
    using Windows.UI.Xaml.Data;

    public class RecStateConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as RecState? ?? RecState.Stopped) == RecState.Stopped ? "\uE20A" : "\uE25D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}