using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace GMaster
{
    public static class Log
    {
        public static TelemetryClient Telemetry;

        static Log()
        {
            var config = new TelemetryConfiguration
            {
                InstrumentationKey = "132c2cbe-e02d-4d36-85bf-efe3bc8ee3e6"
                //TelemetryChannel = new MyChannel()
            };

            Telemetry = new TelemetryClient(config);
        }
        public static void Error(Exception eException)
        {
            Telemetry.TrackException(eException);
        }

        public static void Error(Exception eException, string connect)
        {
        }
    }
}
