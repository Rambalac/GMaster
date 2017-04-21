namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class GreaterToTrueConverter : DelegateParameterConverter<int, int, bool>
    {
        protected override Func<int, int, bool> Converter => (value, param) => value > param;
    }
}