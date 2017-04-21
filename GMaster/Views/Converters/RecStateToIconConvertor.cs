namespace GMaster.Views.Converters
{
    using System;
    using Core.Camera.LumixData;
    using Tools;

    public class RecStateToIconConvertor : DelegateConverter<RecState, string>
    {
        protected override Func<RecState, string> Converter => value => value == RecState.Stopped ? "\uE20A" : "\uE25D";
    }
}