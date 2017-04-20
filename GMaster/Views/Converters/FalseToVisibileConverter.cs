namespace GMaster.Views.Converters
{
    using System;
    using Tools;
    using Windows.UI.Xaml;

    public class FalseToVisibileConverter : DelegateConverter<bool, Visibility>
    {
        protected override Func<bool, Visibility> Converter => value => !value ? Visibility.Visible : Visibility.Collapsed;
    }
}