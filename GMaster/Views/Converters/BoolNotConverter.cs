namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class BoolNotConverter : DelegateConverter<bool, bool>
    {
        protected override Func<bool, bool> Converter => value => !value;
    }
}