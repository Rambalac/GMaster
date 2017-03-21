using System.Net.Sockets;

namespace GMaster
{
    using System;
    using System.Diagnostics;
    using Microsoft.HockeyApp;

    public static class Log
    {
        static Log()
        {
            HockeyClient.Current.Configure("7d7a7144b068445db1eb29135444562a");
        }

        [Conditional("DEBUG")]
        public static void Error(Exception eException)
        {
            HockeyClient.Current.TrackException(eException);
        }

        [Conditional("DEBUG")]
        public static void Trace(string str)
        {
            HockeyClient.Current.TrackTrace(str, SeverityLevel.Information);
        }

        public static void Warn(string str)
        {
            HockeyClient.Current.TrackTrace(str, SeverityLevel.Warning);
        }

        public static void Debug(Exception socketException)
        {
            
        }
    }
}