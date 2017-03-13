using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace GMaster
{
    public static class Log
    {
        private static readonly TelemetryClient Telemetry;

        static Log()
        {
            Telemetry = new TelemetryClient() { InstrumentationKey = "132c2cbe-e02d-4d36-85bf-efe3bc8ee3e6" };
            //TelemetryConfiguration.Active.InstrumentationKey = "132c2cbe-e02d-4d36-85bf-efe3bc8ee3e6";
        }

        [Conditional("DEBUG")]
        public static void Error(Exception eException)
        {
            Telemetry.TrackException(eException);
        }

        [Conditional("DEBUG")]
        public static void Trace(string str)
        {
            Telemetry.TrackTrace(str);
        }
    }
}
