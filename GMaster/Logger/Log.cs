namespace GMaster.Logger
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
        public static void Debug(Exception exception)
        {
            Tools.Debug.WriteLine(exception);
        }

        public static void Error(Exception exception)
        {
            if (exception != null)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                HockeyClient.Current.TrackException(exception);
            }

            Tools.Debug.WriteLine(exception);
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
    }
}