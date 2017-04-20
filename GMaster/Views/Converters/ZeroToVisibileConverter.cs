namespace GMaster.Views.Converters
{
    using System;
    using Tools;
    using Windows.UI.Xaml;

    public class ZeroToVisibileConverter : DelegateConverter<int, Visibility>
    {
        protected override Func<int, Visibility> Converter => value => value == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}