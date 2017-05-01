namespace GMaster.Views.Converters
{
    using System;
    using System.Collections.Generic;
    using Core.Camera.LumixData;
    using Tools;

    public class AutofocusModeToIconPathConverter : DelegateConverter<AutoFocusMode, string>
    {
        private static readonly Dictionary<AutoFocusMode, string> Modes = new Dictionary<AutoFocusMode, string>
        {
            { AutoFocusMode.Face, "Face.png" },
            { AutoFocusMode.FreeMultiArea, "FreeMultiArea.png" },
            { AutoFocusMode.MultiArea, "MultiArea.png" },
            { AutoFocusMode.OneArea, "OneArea.png" },
            { AutoFocusMode.Pinpoint, "Pinpoint.png" },
            { AutoFocusMode.Track, "Track.png" },
        };

        protected override Func<AutoFocusMode, string> Converter => value => Modes.TryGetValue(value, out var res) ? "/images/AutofocusMode/" + res : string.Empty;
    }
}