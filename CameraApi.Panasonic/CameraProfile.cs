namespace CameraApi.Panasonic
{
    using System.Collections.Generic;

    public class CameraProfile
    {
        private static readonly CameraParser GH3Parser = new GH3Parser();
        private static readonly CameraParser GH4Parser = new GH4Parser();

        public static Dictionary<string, CameraProfile> Profiles { get; } = new Dictionary<string, CameraProfile>
        {
            {
                "DMC-GH3", new CameraProfile
                {
                    RecStop = false,
                    NewTouch = false,
                    RequestConnection = false,
                    SetDeviceName = false,
                    Parser = GH3Parser,
                    ManualFocusAF = false
                }
            },
            {
                "DMC-GH4", new CameraProfile
                {
                    RequestConnection = false,
                    SetDeviceName = false,
                    Parser = GH4Parser
                }
            },
            {
                "DMC-GX7", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-GX80", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-GX85", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-G80", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-G7", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-LX100", new CameraProfile
                {
                    SetDeviceName = false
                }
            },
            {
                "DMC-TS5", new CameraProfile
                {
                    SetDeviceName = false,
                    Parser = GH3Parser
                }
            },
            {
                "DMC-GM1", new CameraProfile
                {
                    SetDeviceName = false,
                }
            }
        };

        public bool NewTouch { get; set; } = true;

        public CameraParser Parser { get; set; }

        public bool RecStop { get; set; } = true;

        public bool RequestConnection { get; set; } = true;

        public bool SetDeviceName { get; set; } = true;

        public bool ManualFocusAF { get; set; } = true;
    }
}