namespace GMaster.Core.Camera.Panasonic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using LumixData;
    using Network;
    using Tools;
    using Nito.AsyncEx;

    public partial class Lumix: ICamera
    {
        private static readonly Dictionary<string, RunnableCommandInfo> RunnableCommands;

        private readonly Http http;

        private readonly Timer stateTimer;
        private readonly AsyncLock stateUpdatingLock = new AsyncLock();
        private bool autoreviewUnlocked;
        private CancellationTokenSource connectCancellation = new CancellationTokenSource();
        private bool firstconnect = true;
        private bool isConnecting = true;
        private int isUpdatingState;
        private string language;
        private bool reportingAction;
        private int stateFiledTimes;

        static Lumix()
        {
            RunnableCommands = new Dictionary<string, RunnableCommandInfo>(20);
            foreach (var method in typeof(Lumix).GetRuntimeMethods())
            {
                var runnable = method.GetCustomAttribute<RunnableActionAttribute>();
                if (runnable != null)
                {
                    var rett = method.ReturnType;
                    var info = new RunnableCommandInfo
                    {
                        Method = method,
                        Group = runnable.Group,
                        Async = rett == typeof(Task) || (rett.IsConstructedGenericType && rett.GetGenericTypeDefinition() == typeof(Task<>))
                    };

                    RunnableCommands[method.Name] = info;
                }
            }
        }

        public Lumix(DeviceInfo device, IHttpClient client)
        {
            Device = device;
            Profile = CameraProfile.Profiles.TryGetValue(device.ModelName, out var prof) ? prof : new CameraProfile();

            stateTimer = new Timer(StateTimer_Tick, null, -1, -1);
            var baseUri = new Uri($"http://{CameraHost}/cam.cgi");

            http = new Http(baseUri, client);
        }

        public delegate Task<IntPoint?> LiveViewUpdatedDelegate(ArraySegment<byte> data);

        public event Action<Lumix, string, object[]> ActionCalled;

        public event LiveViewUpdatedDelegate LiveViewUpdated;

        public event Action ProfileUpdated;

        public event Action<ICamera, UpdateStateFailReason> StateUpdateFailed;
        
        public string CameraHost => Device.Host;

        public DeviceInfo Device { get; }

        public LumixState LumixState { get; } = new LumixState();

        public OffFrameProcessor OffFrameProcessor { get; private set; }

        public CameraProfile Profile { get; }

        public string Uuid => Device.Uuid;

        public static MethodGroup GetCommandCroup(string method) => RunnableCommands[method].Group;

        public void Dispose()
        {
            connectCancellation.Cancel();
            connectCancellation.Dispose();
            http.Dispose();
            stateTimer.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj.GetType() == GetType()) && Equals((Lumix)obj);
        }

        public override int GetHashCode()
        {
            return Uuid?.GetHashCode() ?? 0;
        }

        internal async Task ProcessMessage(byte[] buf)
        {
            try
            {
                if (OffFrameProcessor == null)
                {
                    return;
                }

                var slice = new Slice(buf);

                var imageStart = OffFrameProcessor.CalcImageStart(slice);

                if (LiveViewUpdated != null && imageStart > 60 && imageStart < buf.Length - 100 &&
                    buf[imageStart] == 0xff && buf[imageStart + 1] == 0xd8 && buf[buf.Length - 2] == 0xff &&
                    buf[buf.Length - 1] == 0xd9)
                {
                    IntPoint? size = null;
                    foreach (var ev in LiveViewUpdated.GetInvocationList().Cast<LiveViewUpdatedDelegate>())
                    {
                        size = await ev(new ArraySegment<byte>(buf, imageStart, buf.Length - imageStart));
                    }

                    if (size != null)
                    {
                        OffFrameProcessor.Process(new Slice(slice, 0, imageStart), size.Value);
                    }

                    if (firstconnect)
                    {
                        firstconnect = false;
                        LogTrace($"Camera connected and got first frame {Device.ModelName}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (firstconnect)
                {
                    firstconnect = false;
                    LogError($"Camera failed first frame {Device.ModelName}", ex);
                }
            }
        }

        protected bool Equals(Lumix other)
        {
            return Equals(Uuid, other.Uuid);
        }

        private bool CheckAlreadyConnected(string raw)
        {
            if (raw == null)
            {
                return false;
            }

            if (!raw.StartsWith("<xml>"))
            {
                return false;
            }

            try
            {
                var res = Http.ReadResponse<BaseRequestResult>(raw);
                return res.Result == "err_already_connected";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void LogError(string message, string place = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            var placeText = place != null ? $"place.{place}," : string.Empty;
            Log.Error(message, $"{placeText}camera.{Device.ModelName}", fileName, methodName);
        }

        private void LogError(string message, Exception ex, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            Log.Error(message, ex, $"camera.{Device.ModelName}", fileName, methodName);
        }

        private void LogError(string message, object obj, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            Log.Trace(message, Log.Severity.Error, obj, $"camera.{Device.ModelName}", fileName, methodName);
        }

        private void LogError(Exception ex, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            Log.Error(ex.Message, ex, $"camera.{Device.ModelName}", fileName, methodName);
        }

        private void LogTrace(string message, string place = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            var placeText = place != null ? $"place.{place}," : string.Empty;
            Log.Trace(message, Log.Severity.Trace, new { Camera = Device.ModelName }, $"{placeText}camera.{Device.ModelName}", fileName, methodName);
        }

        private async void OffFrameProcessor_LensChanged()
        {
            try
            {
                LumixState.MenuSet = await GetMenuSet();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void ReportAction(string method, object[] prm)
        {
            if (!reportingAction)
            {
                try
                {
                    reportingAction = true;
                    ActionCalled?.Invoke(this, method, prm);
                }
                finally
                {
                    reportingAction = false;
                }
            }
        }

        private void ReportAction(object p1, [CallerMemberName] string method = null)
        {
            ReportAction(method, new[] { p1 });
        }

        private void ReportAction(object p1, object p2, [CallerMemberName] string method = null)
        {
            ReportAction(method, new[] { p1, p2 });
        }

        private void ReportAction(object p1, object p2, object p3, [CallerMemberName] string method = null)
        {
            ReportAction(method, new[] { p1, p2, p3 });
        }

        private void ReportAction([CallerMemberName] string method = null)
        {
            ReportAction(method, new object[0]);
        }

        private async Task<bool> RequestAccess(CancellationToken token)
        {
            var noconnection = 0;
            do
            {
                try
                {
                    var str = await http.GetString($"?mode=accctrl&type=req_acc&value={Device.Uuid}&value2=SM-G9350", token);
                    if (str.StartsWith("<?xml"))
                    {
                        break;
                    }

                    var fields = str.Split(',');
                    if (fields.FirstOrDefault() == "ok")
                    {
                        break;
                    }

                    noconnection = 0;
                }
                catch (COMException ex)
                {
                    Log.Error(ex);
                    if ((uint)ex.HResult == 0x80072efd)
                    {
                        if (++noconnection > 2)
                        {
                            Log.Error("Cannot connect to " + CameraHost);
                            return false;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }

                await Task.Delay(1000, token);
            }
            while (true);

            return true;
        }

        private async void StateTimer_Tick(object sender)
        {
            if (isConnecting)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref isUpdatingState, 1, 0) == 0)
            {
                try
                {
                    var lastState = LumixState.State;
                    var state = LumixState.State = await GetState();
                    if (lastState.Operate != null && state.Operate == null)
                    {
                        Debug.WriteLine("Not connected?", "StateUpdate");
                        StateUpdateFailed?.Invoke(this, UpdateStateFailReason.NotConnected);
                        return;
                    }

                    stateFiledTimes = 0;
                }
                catch (ConnectionLostException)
                {
                    Debug.WriteLine("Connection lost", "Connection");
                    if (++stateFiledTimes > 3)
                    {
                        LogTrace("Connection lost");
                        StateUpdateFailed?.Invoke(this, UpdateStateFailReason.RequestFailed);
                    }
                }
                catch (LumixException ex)
                {
                    Debug.WriteLine(ex);
                    StateUpdateFailed?.Invoke(this, UpdateStateFailReason.LumixException);
                }
                catch (Exception)
                {
                    if (++stateFiledTimes > 3)
                    {
                        StateUpdateFailed?.Invoke(this, UpdateStateFailReason.RequestFailed);
                    }
                }
                finally
                {
                    isUpdatingState = 0;
                }
            }
        }

        private async Task<bool> Try<T>(Func<Task<T>> act)
        {
            try
            {
                await act();
                return true;
            }
            catch (ConnectionLostException)
            {
                Debug.WriteLine("Connection lost", "Connection");
                return false;
            }
            catch (Exception ex)
            {
                LogError("Camera action failed", ex);
                return false;
            }
        }

        private async Task<bool> TryGet(string path)
        {
            return await Try(async () => await http.Get<BaseRequestResult>(path));
        }

        private async Task<bool> TryGetString(string path)
        {
            return await Try(async () =>
            {
                var res = await http.GetString(path);
                Debug.WriteLine(res, "GetString");
                return res;
            });
        }

        private class RunnableCommandInfo
        {
            public bool Async { get; set; }

            public MethodGroup Group { get; set; }

            public MethodInfo Method { get; set; }
        }
    }
}