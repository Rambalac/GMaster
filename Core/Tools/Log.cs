namespace GMaster.Core.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Network;
    using Newtonsoft.Json;
    using Nito.AsyncEx;

    public static class Log
    {
        private static readonly AsyncProducerConsumerQueue<LogEntry> SendQueue = new AsyncProducerConsumerQueue<LogEntry>();
        private static readonly AsyncManualResetEvent SendQueueEmpty = new AsyncManualResetEvent(true);
        private static string baseUri;
        private static CancellationTokenSource cancellation;
        private static IHttpClient http;
        private static ConcurrentQueue<string> memoryEntries;

        private static int memoryLines;

        private static int sendQueueCount = 0;

        private static string version;

        public enum Severity
        {
            Warn,
            Error,
            Debug,
            Trace
        }

        public static void Error(string message, Exception exception, string tags = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            try
            {
                InnerTrace(message, Severity.Error, PrepareException(exception), tags, fileName, methodName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Logger");
            }
        }

        public static void Error(string message, string tags = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null) => Trace(message, Severity.Error, null, tags, fileName, methodName);

        public static void Error(Exception ex, string tags = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null) => Error(ex.Message, ex, tags, fileName, methodName);

        public static void Flush()
        {
            SendQueueEmpty.Wait();
        }

        public static string GetInmemoryMessages()
        {
            return string.Join("\r\n", memoryEntries);
        }

        public static void Init(IHttpClient client, string token, string ver, int inmemory = 0)
        {
            version = ver.Replace('.', '_');
            http = client;
            memoryLines = inmemory;
            if (memoryLines != 0)
            {
                memoryEntries = new ConcurrentQueue<string>();
            }

            baseUri = $"http://logs-01.loggly.com/inputs/{token}";
            cancellation = new CancellationTokenSource();
            Task.Factory.StartNew(Sender, TaskCreationOptions.LongRunning);
        }

        public static void Stop()
        {
            cancellation.Cancel();
        }

        public static void Trace(string message, Severity severity = Severity.Trace, object data = null, string tags = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            try
            {
                InnerTrace(message, severity, data, tags, fileName, methodName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public static void Warn(string str, string tags = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null) => Trace(str, Severity.Warn, null, tags, fileName, methodName);

        private static void InnerTrace(string message, Severity severity, object data, string tags, string fileName, string methodName)
        {
            Debug.WriteLine(severity + ": " + message, tags);

            if (data != null)
            {
                Debug.WriteLine(data.ToString(), tags);
            }

            var logglyMessage = new LogglyMessage
            {
                Message = message,
                File = fileName,
                Method = methodName,
                Data = data
            };

            var str = JsonConvert.SerializeObject(
                logglyMessage,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            if (memoryEntries != null)
            {
                memoryEntries.Enqueue(str);
                if (memoryEntries.Count > memoryLines)
                {
                    memoryEntries.TryDequeue(out _);
                }
            }

            var tagsText = tags != null ? "," + tags : string.Empty;
            var entry = new LogEntry
            {
                Message = str,
                Tags = $"severity.{severity},version.{version}{tagsText}"
            };

            SendQueueEmpty.Reset();
            Interlocked.Increment(ref sendQueueCount);
            SendQueue.Enqueue(entry);
        }

        private static ExceptionInfo PrepareException(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            var result = new ExceptionInfo
            {
                Exception = exception.GetType().FullName,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };

            switch (exception)
            {
                case AggregateException aggr:
                    result.InnerExceptions = aggr.InnerExceptions?.Select(PrepareException).ToArray();
                    break;

                default:
                    result.InnerExceptions = exception.InnerException != null ? new[] { PrepareException(exception.InnerException) } : null;
                    break;
            }

            return result;
        }

        private static async Task Sender()
        {
            try
            {
                var token = cancellation.Token;
                while (true)
                {
                    try
                    {
                        var entry = await SendQueue.DequeueAsync(token);
                        if (Interlocked.Decrement(ref sendQueueCount) == 0)
                        {
                            SendQueueEmpty.Set();
                        }

                        if (entry == null)
                        {
                            continue;
                        }

                        var result = await http.PostStringAsync(new Uri(baseUri + $"/tag/{Uri.EscapeDataString(entry.Tags)}"), entry.Message, token);
                        if (result != "{\"response\" : \"ok\"}")
                        {
                            Debug.WriteLine(result, "Loggly");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                cancellation.Dispose();
            }
        }

        private class ExceptionInfo
        {
            [JsonProperty(Order = 1)]
            public string Exception { get; set; }

            [JsonProperty(Order = 4)]
            public ExceptionInfo[] InnerExceptions { get; set; }

            [JsonProperty(Order = 2)]
            public string Message { get; set; }

            [JsonProperty(Order = 3)]
            public string StackTrace { get; set; }

            public override string ToString()
            {
                return Message + "\r\n" + StackTrace;
            }
        }

        private class LogEntry
        {
            public string Message { get; set; }

            public string Tags { get; set; }
        }

        private class LogglyMessage
        {
            [JsonProperty(Order = 4)]
            public object Data { get; set; }

            [JsonProperty(Order = 3)]
            public string File { get; set; }

            [JsonProperty(Order = 1)]
            public string Message { get; set; }

            [JsonProperty(Order = 2)]
            public string Method { get; set; }
        }
    }
}