namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using LumixData;
    using Network;
    using Nito.AsyncEx;
    using Tools;

    public class Lumix : IDisposable
    {
        private readonly Http http;
        private readonly CameraProfile profile;
        private readonly Timer stateTimer;
        private readonly AsyncLock stateUpdatingLock = new AsyncLock();
        private CancellationTokenSource connectCancellation = new CancellationTokenSource();
        private bool firstconnect = true;
        private bool isConnecting = true;
        private int isUpdatingState;
        private string language;
        private int stateFiledTimes;

        public Lumix(DeviceInfo device, IHttpClient client)
        {
            Device = device;
            profile = CameraProfile.Profiles.TryGetValue(device.ModelName, out var prof) ? prof : new CameraProfile();

            stateTimer = new Timer(StateTimer_Tick, null, -1, -1);
            var baseUri = new Uri($"http://{CameraHost}/cam.cgi");

            http = new Http(baseUri, client);
        }

        public delegate Task<CameraPoint?> LiveViewUpdatedDelegate(ArraySegment<byte> data);

        public event LiveViewUpdatedDelegate LiveViewUpdated;

        public event Action<Lumix, UpdateStateFailReason> StateUpdateFailed;

        public enum UpdateStateFailReason
        {
            RequestFailed,
            LumixException,
            NotConnected
        }

        public string CameraHost => Device.Host;

        public DeviceInfo Device { get; }

        public LumixState LumixState { get; } = new LumixState();

        public OffFrameProcessor OffFrameProcessor { get; private set; }

        public CameraParser Parser { get; private set; }

        public string Uuid => Device.Uuid;

        public async Task<bool> Capture()
        {
            return await Try(async () =>
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture");
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
                return true;
            });
        }

        public async Task<bool> ChangeFocus(ChangeDirection dir)
        {
            return await Try(async () =>
            {
                var focus = await http.GetString("?mode=camctrl&type=focus&value=" + dir.GetString());

                var fp = Parser.ParseFocus(focus);
                if (fp == null)
                {
                    return false;
                }

                if (fp.Maximum != LumixState.MaximumFocus)
                {
                    LumixState.MaximumFocus = fp.Maximum;
                }

                if (fp.Value != LumixState.CurrentFocus)
                {
                    LumixState.CurrentFocus = fp.Value;
                }

                return true;
            });
        }

        public async Task<bool> ChangeZoom(ChangeDirection focus)
        {
            return await Try(async () => await http.Get<BaseRequestResult>("?mode=camcmd&value=" + focus.GetString()));
        }

        public async Task<bool> Connect(int liveviewport, string lang)
        {
            language = lang;
            var token = connectCancellation.Token;
            var connectStage = 0;
            try
            {
                LogTrace("Connecting camera " + Device.ModelName);
                do
                {
                    try
                    {
                        if (profile.RequestConnection)
                        {
                            await RequestAccess(token);
                        }

                        connectStage = 1;

                        if (profile.SetDeviceName)
                        {
                            await TryGet("?mode=setsetting&type=device_name&value=SM-G9350");
                        }

                        connectStage = 2;

                        LumixState.Reset();

                        LumixState.MenuSet = await GetMenuSet();
                        if (LumixState.MenuSet == null)
                        {
                            LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", "CameraConnect");
                            LumixState.IsLimited = true;
                        }

                        if (!LumixState.IsLimited)
                        {
                            LumixState.CurMenu = await GetCurMenu();
                        }

                        connectStage = 3;
                        await SwitchToRec();

                        connectStage = 4;
                        LumixState.LensInfo = await GetLensInfo();

                        connectStage = 5;
                        LumixState.State = await GetState();

                        connectStage = 6;
                        await http.Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}", token);

                        token.ThrowIfCancellationRequested();
                        break;
                    }
                    catch (ConnectionLostException)
                    {
                        Debug.WriteLine("Connection lost", "Connection");
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("Timeout", "Connection");
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", ex);
                    }

                    await Task.Delay(1000, token);
                }
                while (true);

                if (OffFrameProcessor == null)
                {
                    connectStage = 7;
                    OffFrameProcessor = new OffFrameProcessor(Device.ModelName, Parser, LumixState);
                    OffFrameProcessor.LensChanged += OffFrameProcessor_LensChanged;
                }

                stateTimer.Change(2000, 2000);
                return true;
            }
            catch (OperationCanceledException)
            {
                connectCancellation.Dispose();
                return false;
            }
            catch (Exception e)
            {
                LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", e);
                return false;
            }
            finally
            {
                isConnecting = false;
            }
        }

        public void Disconnect()
        {
            try
            {
                connectCancellation.Cancel();
                connectCancellation = new CancellationTokenSource();

                stateTimer.Change(-1, -1);
                Debug.WriteLine("Timer stopped", "Disconnect");
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

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

            return obj.GetType() == GetType() && Equals((Lumix)obj);
        }

        public async Task<FocusMode> GetFocusMode()
        {
            try
            {
                var result = await http.Get<FocusModeRequestResult>("?mode=getsetting&type=focusmode");
                return result.Value.FocusMode;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return FocusMode.Unknown;
            }
        }

        public override int GetHashCode()
        {
            return Uuid?.GetHashCode() ?? 0;
        }

        public async Task<bool> RecStart()
        {
            return await Try(async () =>
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstart");
                if (profile.RecStop)
                {
                    LumixState.RecState = RecState.Unknown;
                    await Task.Delay(100);
                    LumixState.State = await GetState();
                    return true;
                }
                else
                {
                    LumixState.RecState = RecState.StopNotSupported;
                }

                return true;
            });
        }

        public async Task<bool> RecStop()
        {
            if (profile.RecStop)
            {
                try
                {
                    await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
                    await Task.Delay(500);
                    LumixState.State = await GetState();
                    return true;
                }
                catch (LumixException ex)
                {
                    if (ex.Error == LumixError.ErrorParam)
                    {
                        Debug.WriteLine("RecStop not supported", "RecStop");
                        profile.RecStop = false;
                        LumixState.RecState = RecState.StopNotSupported;
                    }
                    else
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            return false;
        }

        public async Task<bool> ResizeFocusPoint(int size)
        {
            return await Try(async () =>
            {
                if (size > 0)
                {
                    await http.Get<BaseRequestResult>("?mode=camctrl&type=touchaf_chg_area&value=up");
                }
                else if (size < 0)
                {
                    await http.Get<BaseRequestResult>("?mode=camctrl&type=touchaf_chg_area&value=down");
                }

                return true;
            });
        }

        public async Task<bool> SendMenuItem(ICameraMenuItem value)
        {
            if (value != null)
            {
                return await Try(async () =>
                    await http.Get<BaseRequestResult>(new Dictionary<string, string>
                    {
                        { "mode", value.Command },
                        { "type", value.CommandType },
                        { "value", value.Value }
                    }));
            }

            return false;
        }

        public async Task<bool> SetFocusPoint(double x, double y)
        {
            return await Try(async () =>
            {
                if (profile.NewAf)
                {
                    try
                    {
                        var onoff = "on"; // "off"
                        await http.Get<BaseRequestResult>(
                            $"?mode=camctrl&type=touch&value={(int)(x * 1000)}/{(int)(y * 1000)}&value2={onoff}");
                        return true;
                    }
                    catch (LumixException ex)
                    {
                        if (ex.Error == LumixError.ErrorParam)
                        {
                            LogTrace("New TouchAF not supported", "NewTouchAF");
                            profile.NewAf = false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                await http.Get<BaseRequestResult>(
                    $"?mode=camctrl&type=touchaf&value={(int)(x * 1000)}/{(int)(y * 1000)}");
                return true;
            });
        }

        public async Task StopStream()
        {
            if (!isConnecting)
            {
                await Try(async () => await http.Get<BaseRequestResult>("?mode=stopstream"));
            }
        }

        public async Task<bool> SwitchToRec()
        {
            return await Try(async () => await http.Get<BaseRequestResult>("?mode=camcmd&value=recmode"));
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
                    CameraPoint? size = null;
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

        private async Task<CurMenu> GetCurMenu()
        {
            var curmenuString = await http.GetString("?mode=getinfo&type=curmenu");
            var response = Http.ReadResponse<CurMenuRequestResult>(curmenuString);

            if (response.MenuInfo == null)
            {
                return null;
            }

            try
            {
                var result = Parser.ParseCurMenu(response.MenuInfo);
                return result;
            }
            catch (AggregateException)
            {
                LogError("Cannot parse CurMenu", (object)curmenuString);
                return null;
            }
        }

        private async Task<LensInfo> GetLensInfo()
        {
            string raw = null;
            try
            {
                raw = await http.GetString("?mode=getinfo&type=lens");
                return Parser.ParseLensInfo(raw);
            }
            catch (Exception)
            {
                Debug.WriteLine("LensInfo: " + raw, "LensInfo");
                throw;
            }
        }

        private async Task<MenuSet> GetMenuSet()
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=allmenu");
            var result = Http.ReadResponse<MenuSetRequestResult>(allmenuString);

            if (result.MenuSet == null)
            {
                return null;
            }

            try
            {
                if (Parser == null)
                {
                    Parser = CameraParser.TryParseMenuSet(result.MenuSet, language, out var menuset);
                    return menuset;
                }

                return Parser.ParseMenuSet(result.MenuSet, language);
            }
            catch (AggregateException)
            {
                LogError("Cannot parse MenuSet", (object)allmenuString);
                return null;
            }
        }

        private async Task<CameraState> GetState()
        {
            var response = await http.Get<CameraStateRequestResult>("?mode=getstate");
            var newState = response.State;
            if (newState.Rec == OnOff.On)
            {
                LumixState.RecState = profile.RecStop ? RecState.Started : RecState.StopNotSupported;
            }
            else
            {
                LumixState.RecState = RecState.Stopped;
            }

            return newState;
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
            Log.Trace(message, Log.Severity.Trace, null, $"{placeText}camera.{Device.ModelName}", fileName, methodName);
        }

        private async void OffFrameProcessor_LensChanged()
        {
            LumixState.MenuSet = await GetMenuSet();
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
    }
}