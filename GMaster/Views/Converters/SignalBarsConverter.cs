namespace GMaster.Views.Converters
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class SignalBarsConverter : IValueConverter
    {
        private static readonly string[] Bars = new[] { " ", "\xE86C", "\xE86D", "\xE86E", "\xE86F", "\xE870" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Bars[(byte)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}