namespace GMaster.Core.Camera
{
    using System.Collections.Generic;

    public class CameraProfile
    {
        private static CameraParser gh3Parser = new GH3Parser();
        private static CameraParser gh4Parser = new GH4Parser();
        public static Dictionary<string, CameraProfile> Profiles { get; } = new Dictionary<string, CameraProfile>
        {
            {
                "DMC-GH3", new CameraProfile
                {
                    RecStop = false,
                    NewAf = false,
                    RequestConnection = false,
                    SetDeviceName = false,
                    Parser = gh3Parser
                }
            },
            {
                "DMC-GH4", new CameraProfile
                {
                    RequestConnection = false,
                    SetDeviceName = false,
                    Parser = gh4Parser
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
                    Parser = gh3Parser
                }
            }
        };

        public CameraParser Parser { get; set; }

        public bool NewAf { get; set; } = true;

        public bool RecStop { get; set; } = true;

        public bool RequestConnection { get; set; } = true;

        public bool SetDeviceName { get; set; } = true;
    }
}