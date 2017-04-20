namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class NullToTrueConverter : DelegateConverter<object, bool>
    {
        protected override Func<object, bool> Converter => value => value == null;
    }
}