namespace GMaster.Views.Converters
{
    using System;
    using Tools;
    using Windows.UI.Xaml;

    public class NullToVisibleConverter : DelegateConverter<object, Visibility>
    {
        protected override Func<object, Visibility> Converter => value => value == null ? Visibility.Visible : Visibility.Collapsed;
    }
}