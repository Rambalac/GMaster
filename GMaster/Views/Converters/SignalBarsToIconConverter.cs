namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class SignalBarsToIconConverter : DelegateConverter<byte, string>
    {
        private static readonly string[] Bars = { " ", "\xE86C", "\xE86D", "\xE86E", "\xE86F", "\xE870" };

        protected override Func<byte, string> Converter => value => Bars[value];
    }
}