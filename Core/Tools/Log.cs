namespace GMaster.Core.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
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

        static Log()
        {
        }

        public enum Severity
        {
            Warn,
            Error,
            Debug,
            Trace
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugError(Exception exception)
        {
            Debug.WriteLine(exception);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugTrace(string str)
        {
        }

        public static void Error(string message, Exception exception, string tags = null)
        {
            try
            {
                InnerTrace(message, Severity.Error, PrepareException(exception), tags);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Logger");
            }
        }

        public static void Error(string message, string tags = null) => Trace(message, Severity.Error, tags: tags);

        public static void Error(Exception ex) => Error(ex.Message, ex);

        public static void Flush()
        {
            SendQueueEmpty.Wait();
        }

        public static string GetInmemoryMessages()
        {
            return string.Join("\r\n", memoryEntries);
        }

        public static void Init(IHttpClient client, string token, int inmemory = 0)
        {
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

        public static void Trace(string message, Severity severity = Severity.Trace, object data = null, string tags = null)
        {
            try
            {
                InnerTrace(message, severity, data, tags);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public static void Warn(string str, string tags = null) => Trace(str, Severity.Warn, null, tags);

        private static void InnerTrace(string message, Severity severity = Severity.Trace, object data = null, string tags = null)
        {
            Debug.WriteLine(severity + ": " + message, tags);

            if (data != null)
            {
                Debug.WriteLine(data.ToString(), tags);
            }

            var logglyMessage = new LogglyMessage
            {
                Message = message,
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

            var entry = new LogEntry
            {
                Message = str,
                Tags = $"severity.{severity},{tags ?? string.Empty}"
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

                        var result = await http.PostStringAsync(new Uri(baseUri + $"/tag/{Uri.EscapeDataString(entry.Tags)}"), entry.Message);
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
            public ExceptionInfo[] InnerExceptions { get; set; }

            public string Message { get; set; }

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
            public object Data { get; set; }

            public string Message { get; set; }
        }
    }
}