using GMaster.Core.Camera.Panasonic.LumixData;

namespace GMaster.Views.Converters
{
    using System;
    using Tools;

    public class RecStateToIconConvertor : DelegateConverter<RecState, string>
    {
        protected override Func<RecState, string> Converter => value => value == RecState.Stopped ? "\uE20A" : "\uE25D";
    }
}