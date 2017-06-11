using GMaster.Core.Camera.Panasonic.LumixData;

namespace GMaster.Views.Converters
{
    using System;
    using System.Collections.Generic;
    using Tools;

    public class CameraModeToIconPathConverter : DelegateConverter<CameraMode, string>
    {
        private static readonly Dictionary<CameraMode, string> Modes = new Dictionary<CameraMode, string>
        {
            { CameraMode.A, "PhotoA.png" },
            { CameraMode.M, "PhotoM.png" },
            { CameraMode.S, "PhotoS.png" },
            { CameraMode.P, "PhotoP.png" },
            { CameraMode.vA, "MovieA.png" },
            { CameraMode.vM, "MovieM.png" },
            { CameraMode.vS, "MovieS.png" },
            { CameraMode.vP, "MovieP.png" },
        };

        protected override Func<CameraMode, string> Converter => value => Modes.TryGetValue(value, out var res) ? "/images/CameraMode/" + res : string.Empty;
    }
}