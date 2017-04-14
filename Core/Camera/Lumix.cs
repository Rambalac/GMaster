namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using LumixData;
    using Network;
    using Nito.AsyncEx;
    using Tools;

    public class Lumix : INotifyPropertyChanged, IDisposable
    {
        private const int StateFailedTimesOk = 3;
        private static readonly HashSet<string> RecStopNotSupported = new HashSet<string> { "DMC-GH3" };
        private readonly CancellationTokenSource connectCancellation = new CancellationTokenSource();
        private readonly Http http;

        private readonly Timer stateTimer;

        private readonly AsyncLock stateUpdatingLock = new AsyncLock();
        private bool firstconnect = true;
        private bool isConnecting = true;
        private int isUpdatingState;
        private RecState recState = RecState.Unknown;
        private bool recStopSupported = true;
        private int stateFiledTimes;
        private bool useNewTouchAF = true;

        public Lumix(DeviceInfo device, IHttpClient client)
        {
            Device = device;
            stateTimer = new Timer(StateTimer_Tick, null, -1, -1);
            var baseUri = new Uri($"http://{CameraHost}/cam.cgi");

            http = new Http(baseUri, client);
        }

        public delegate void DisconnectedDelegate(Lumix sender, bool stillAvailable);

        public event DisconnectedDelegate Disconnected;

        public event Func<ArraySegment<byte>, Task<CameraPoint?>> LiveViewUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CameraHost => Device.Host;

        public bool CanCapture { get; private set; } = true;

        public bool CanChangeAperture { get; private set; } = true;

        public bool CanChangeShutter { get; private set; } = true;

        public bool CanManualFocus { get; set; }

        public ICameraMenuItem CurrentAperture => CurrentApertures.FirstOrDefault(
                s => s.IntValue == OffFrameProcessor.Aperture.Bin);

        public ICollection<CameraMenuItem256> CurrentApertures { get; private set; } = new List<CameraMenuItem256>();

        public int CurrentFocus { get; private set; }

        public ICameraMenuItem CurrentIso => MenuSet.IsoValues.FirstOrDefault(
                s => s.Value == OffFrameProcessor.Iso.Text);

        public ICameraMenuItem CurrentShutter => MenuSet.ShutterSpeeds.FirstOrDefault(
                s => s.Value == OffFrameProcessor.Shutter.Bin + "/256");

        public DeviceInfo Device { get; }

        public bool IsConnected { get; private set; }

        public bool IsConnecting
        {
            get => isConnecting;
            private set
            {
                isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
            }
        }

        public bool IsLimited => MenuSet == null;

        public bool IsVideoMode { get; private set; }

        public LensInfo LensInfo { get; private set; }

        public int MaximumFocus { get; private set; }

        public MenuSet MenuSet { get; private set; }

        public OffFrameProcessor OffFrameProcessor { get; private set; }

        public CameraParser Parser { get; private set; }

        public RecState RecState
        {
            get => recState;
            private set
            {
                if (value == recState)
                {
                    return;
                }

                recState = value;
                OnSelfChanged();
            }
        }

        public CameraState State { get; private set; }

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

                if (fp.Maximum != MaximumFocus)
                {
                    MaximumFocus = fp.Maximum;
                    OnPropertyChanged(nameof(MaximumFocus));
                }

                if (fp.Value != CurrentFocus)
                {
                    CurrentFocus = fp.Value;
                    OnPropertyChanged(nameof(CurrentFocus));
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
            var token = connectCancellation.Token;
            var connectStage = 0;
            try
            {
                LogTrace("Connecting camera " + Device.ModelName);

                recStopSupported = !RecStopNotSupported.Contains(Device.ModelName);
                do
                {
                    try
                    {
                        await RequestAccess(token);

                        connectStage = 1;

                        await TryGet("?mode=setsetting&type=device_name&value=SM-G9350");

                        connectStage = 2;

                        if (!await ReadMenuSet(lang))
                        {
                            LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", "CameraConnect");
                            return false;
                        }

                        connectStage = 3;

                        token.ThrowIfCancellationRequested();
                        break;
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

                OffFrameProcessor = new OffFrameProcessor(Device.ModelName, Parser);
                OffFrameProcessor.PropertyChanged += OffFrameProcessor_PropertyChanged;
                OffFrameProcessor.LensUpdated += OfframeProcessor_LensUpdated;
                OnPropertyChanged(nameof(OffFrameProcessor));

                connectStage = 4;
                await SwitchToRec();

                connectStage = 5;
                await UpdateState();
                stateTimer.Change(2000, 2000);

                connectStage = 6;
                await ReadLensInfo();

                connectStage = 7;
                await http.Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}");

                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", e);
                return false;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public async Task Disconnect(bool stillAvailbale = true)
        {
            try
            {
                connectCancellation.Cancel();
                IsConnected = false;
                stateTimer.Change(-1, -1);
                Debug.WriteLine("Timer stopped", "Disconnect");
                using (var timeout = new CancellationTokenSource(1000))
                {
                    if (!isConnecting)
                    {
                        await Try(async () => await http.Get<BaseRequestResult>("?mode=stopstream", timeout.Token));
                    }

                    Disconnected?.Invoke(this, stillAvailbale);
                    Debug.WriteLine("Disconnected called", "Disconnect");
                }

                Dispose();
            }
            catch (Exception e)
            {
                LogError(e);
            }
            finally
            {
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public void Dispose()
        {
            connectCancellation.Cancel();
            connectCancellation.Dispose();
            http.Dispose();
            OffFrameProcessor.PropertyChanged -= OffFrameProcessor_PropertyChanged;
            OffFrameProcessor.LensUpdated -= OfframeProcessor_LensUpdated;
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
                if (recStopSupported)
                {
                    await Task.Delay(100);
                    await UpdateState();
                    return true;
                }

                RecState = RecState.Unknown;
                return true;
            });
        }

        public async Task<bool> RecStop()
        {
            if (recStopSupported)
            {
                try
                {
                    await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
                    await Task.Delay(500);
                    await UpdateState();
                    return true;
                }
                catch (LumixException ex)
                {
                    if (ex.Error == LumixError.ErrorParam)
                    {
                        Debug.WriteLine("RecStop not supported", "RecStop");
                        recStopSupported = false;
                        RecState = RecState.StopNotSupported;
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
                if (useNewTouchAF)
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
                            useNewTouchAF = false;
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
                    foreach (var ev in LiveViewUpdated.GetInvocationList())
                    {
                        size =
                            await ((Func<ArraySegment<byte>, Task<CameraPoint?>>)ev)(
                                new ArraySegment<byte>(buf, imageStart, buf.Length - imageStart));
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

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSelfChanged([CallerMemberName] string propertyName = null) => OnPropertyChanged(propertyName);

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

        private void OffFrameProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OffFrameProcessor.FocusMode):
                    CanManualFocus = OffFrameProcessor.FocusMode == FocusMode.Manual;
                    OnPropertyChanged(nameof(CanManualFocus));
                    break;

                case nameof(OffFrameProcessor.CameraMode):
                    CanChangeAperture = OffFrameProcessor.CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Aperture);
                    CanChangeShutter = OffFrameProcessor.CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Shutter);
                    CanCapture = OffFrameProcessor.CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Photo);
                    IsVideoMode = OffFrameProcessor.CameraMode.ToValue<CameraModeFlags>().HasFlag(CameraModeFlags.Video);
                    OnPropertyChanged(nameof(CanCapture));
                    OnPropertyChanged(nameof(CanChangeAperture));
                    OnPropertyChanged(nameof(CanChangeShutter));
                    break;

                case nameof(OffFrameProcessor.OpenedAperture):
                    UpdateCurrentApertures();
                    break;
            }
        }

        private async void OfframeProcessor_LensUpdated()
        {
            await Try(ReadLensInfo);
        }

        private async Task ReadCurMenu()
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=curmenu");
            var result = Http.ReadResponse<MenuSetRequestResult>(allmenuString);

            try
            {
                // MenuSet = MenuSet.TryParseMenuSet(result.MenuSet, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            }
            catch (AggregateException)
            {
                LogError("Cannot parse MenuSet", (object)allmenuString);
            }

            await http.Get<BaseRequestResult>("?mode=getinfo&type=curmenu");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        private async Task ReadLensInfo()
        {
            string raw = null;
            try
            {
                raw = await http.GetString("?mode=getinfo&type=lens");
                var newInfo = Parser.ParseLensInfo(raw);
                if (!Equals(newInfo, LensInfo))
                {
                    LensInfo = newInfo;
                    UpdateCurrentApertures();

                    OnPropertyChanged(nameof(LensInfo));
                    OnPropertyChanged(nameof(CurrentApertures));
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("LensInfo: " + raw, "LensInfo");
                throw;
            }
        }

        private async Task<bool> ReadMenuSet(string lang)
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=allmenu");
            var result = Http.ReadResponse<MenuSetRequestResult>(allmenuString);

            if (result.MenuSet == null)
            {
                return false;
            }

            try
            {
                Parser = CameraParser.TryParseMenuSet(result.MenuSet, lang, out var menuset);
                MenuSet = menuset;
                OnPropertyChanged(nameof(MenuSet));
                return true;
            }
            catch (AggregateException)
            {
                LogError("Cannot parse MenuSet", (object)allmenuString);
                return false;
            }
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
            if (!IsConnected)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref isUpdatingState, 1, 0) == 0)
            {
                try
                {
                    var lastState = State;
                    var state = await UpdateState();
                    if (lastState.Operate != null && state.Operate == null)
                    {
                        await Disconnect(false);
                    }

                    stateFiledTimes = 0;
                }
                catch (LumixException ex)
                {
                    Debug.WriteLine(ex);
                    await Disconnect(false);
                }
                catch (Exception)
                {
                    stateFiledTimes++;
                    if (stateFiledTimes > 30)
                    {
                        await Disconnect(false);
                    }
                }
                finally
                {
                    isUpdatingState = 0;
                }
            }
        }

        private async Task<bool> Try(Func<Task> act)
        {
            try
            {
                await act();
                return true;
            }
            catch (Exception ex)
            {
                LogError("Camera action failed", ex);
                return false;
            }
        }

        private async Task<bool> Try<T>(Func<Task<T>> act)
        {
            try
            {
                await act();
                return true;
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

        private void UpdateCurrentApertures()
        {
            var open = OffFrameProcessor?.OpenedAperture ?? LensInfo.OpenedAperture;

            var newCurrentApertures = new List<CameraMenuItem256>(MenuSet.Apertures.Count);
            var opentext = CameraParser.ApertureBinToText(open);
            newCurrentApertures.Add(new CameraMenuItem256(open.ToString(), opentext, "setsetting", "shtrspeed", open));
            newCurrentApertures.AddRange(MenuSet.Apertures.Where(a => a.IntValue >= open && a.Text != opentext && (LensInfo == null || a.IntValue <= LensInfo.ClosedAperture)));
            if (newCurrentApertures.Count != CurrentApertures.Count ||
                newCurrentApertures.First().Value != CurrentApertures.First().Value)
            {
                CurrentApertures = newCurrentApertures;
                OnPropertyChanged(nameof(CurrentApertures));
            }
        }

        private async Task<CameraState> UpdateState()
        {
            using (var timeout = new CancellationTokenSource(1000))
            {
                var newState = await http.Get<CameraStateRequestResult>("?mode=getstate", timeout.Token);
                if (State != null && newState.State.Equals(State))
                {
                    return State;
                }

                State = newState.State ?? throw new NullReferenceException();

                RecState = State.Rec == OnOff.On
                    ? (recStopSupported ? RecState.Started : RecState.StopNotSupported)
                    : RecState.Stopped;

                OnPropertyChanged(nameof(State));
                return State;
            }
        }
    }
}