namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class BatteryToIconConverter : DelegateConverter<float, string>
    {
        private static readonly string[] Battery = { "\xE850", "\xE851", "\xE852", "\xE853", "\xE854", "\xE855", "\xE856", "\xE857", "\xE858", "\xE859", "\xE83F" };

        protected override Func<float, string> Converter => value => Battery[Math.Max(0, (int)Math.Round(value * 10f))];
    }
}