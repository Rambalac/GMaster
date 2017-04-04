namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;
    using LumixData;
    using Tools;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml;

    public class Lumix : INotifyPropertyChanged, IDisposable
    {
        private static readonly HashSet<CameraMode> ApertureModes = new HashSet<CameraMode> { CameraMode.M, CameraMode.A, CameraMode.vM, CameraMode.vA };
        private static readonly HashSet<CameraMode> CaptureModes = new HashSet<CameraMode> { CameraMode.M, CameraMode.S, CameraMode.A, CameraMode.P, CameraMode.Unknown, CameraMode.iA };
        private static readonly HashSet<CameraMode> ShutterModes = new HashSet<CameraMode> { CameraMode.M, CameraMode.S, CameraMode.vM, CameraMode.vS };
        private readonly Http http;
        private readonly object messageRecieving = new object();

        private readonly DispatcherTimer stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        private readonly SemaphoreSlim stateUpdatingSem = new SemaphoreSlim(1);

        private MemoryStream currentImageStream;
        private byte lastByte;

        private MemoryStream offframeBytes;

        private bool useNewTouchAF = true;

        public Lumix(DeviceInfo device)
        {
            Device = device;
            var baseUri = new Uri($"http://{CameraHost}/cam.cgi");
            stateTimer.Tick += StateTimer_Tick;

            http = new Http(baseUri);
        }

        public delegate void DisconnectedDelegate(Lumix sender, bool stillAvailable);

        public event DisconnectedDelegate Disconnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CameraHost => Device.Host;

        public bool CanCapture { get; set; } = true;

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

        public bool IsLimited => MenuSet == null;

        public LensInfo LensInfo { get; private set; }

        public Stream LiveViewFrame { get; private set; }

        public int MaximumFocus { get; private set; }

        public MenuSet MenuSet { get; private set; }

        public OffFrameProcessor OffFrameProcessor { get; private set; }

        public CameraParser Parser { get; private set; }

        public RecState RecState { get; private set; } = RecState.Unknown;

        public CameraState State { get; private set; }

        public string Udn => Device.Udn;

        public async Task<bool> Capture()
        {
            try
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture");
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> ChangeFocus(ChangeDirection dir)
        {
            try
            {
                var focus = await http.GetString("?mode=camctrl&type=focus&value=" + dir.GetString());
                Debug.WriteLine($"Focusing {dir} {focus}");

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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> ChangeZoom(ChangeDirection focus)
        {
            try
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=" + focus.GetString());
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> Connect(int liveviewport, string lang)
        {
            try
            {
                currentImageStream = null;
                lastByte = 0;

                if (!await ReadMenuSet(lang))
                {
                    return false;
                }

                await ReadLensInfo();

                OffFrameProcessor = new OffFrameProcessor(Device.ModelName, Parser);
                OffFrameProcessor.PropertyChanged += OffFrameProcessor_PropertyChanged;
                OffFrameProcessor.LensUpdated += OfframeProcessor_LensUpdated;

                await SwitchToRec();
                await UpdateState();
                stateTimer.Start();
                await http.Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}");

                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task Disconnect()
        {
            try
            {
                IsConnected = false;
                stateTimer.Stop();
                try
                {
                    await http.Get<BaseRequestResult>("?mode=stopstream");
                    Disconnected?.Invoke(this, true);
                }
                catch (Exception)
                {
                    Disconnected?.Invoke(this, false);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public void Dispose()
        {
            stateUpdatingSem.Dispose();
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
            return Udn?.GetHashCode() ?? 0;
        }

        public void ManagerRestarted()
        {
            lock (messageRecieving)
            {
                currentImageStream = null;
                lastByte = 0;
            }
        }

        public async Task<bool> RecStart()
        {
            try
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstart");
                RecState = RecState.Unknown;
                OnPropertyChanged(nameof(RecState));
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> RecStop()
        {
            try
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
                RecState = RecState.Unknown;
                OnPropertyChanged(nameof(RecState));
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> ResizeFocusPoint(int size)
        {
            try
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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task SendMenuItem(ICameraMenuItem value)
        {
            if (value != null)
            {
                await http.Get<BaseRequestResult>(new Dictionary<string, string>
                {
                    { "mode", value.Command },
                    { "type", value.CommandType },
                    { "value", value.Value }
                });
            }
        }

        public async Task<bool> SetFocusPoint(double x, double y)
        {
            try
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
                            Debug.WriteLine("New TouchAF not supported");
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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> SwitchToRec()
        {
            try
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=recmode");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        internal void ProcessMessage(DataReader reader)
        {
            lock (messageRecieving)
            {
                using (reader)
                {
                    while (reader.UnconsumedBufferLength > 0)
                    {
                        var curByte = reader.ReadByte();
                        ProcessByte(curByte);
                    }
                }
            }
        }

        protected bool Equals(Lumix other)
        {
            return Equals(Udn, other.Udn);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnSelfChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    CanChangeAperture = ApertureModes.Contains(OffFrameProcessor.CameraMode);
                    CanChangeShutter = ShutterModes.Contains(OffFrameProcessor.CameraMode);
                    CanCapture = CaptureModes.Contains(OffFrameProcessor.CameraMode);
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
            await TryReadLensInfo();
        }

        private void ProcessByte(byte curByte)
        {
            currentImageStream?.WriteByte(curByte);

            offframeBytes?.WriteByte(curByte);

            if (lastByte == 0xff)
            {
                if (curByte == 0xd8)
                {
                    OffFrameProcessor.Process(offframeBytes);
                    offframeBytes = null;
                    currentImageStream = new MemoryStream(32768);
                    currentImageStream.WriteByte(0xff);
                    currentImageStream.WriteByte(0xd8);
                }
                else if (currentImageStream != null && curByte == 0xd9)
                {
                    LiveViewFrame = currentImageStream;
                    App.RunAsync(() => OnPropertyChanged(nameof(LiveViewFrame)));

                    currentImageStream = null;

                    offframeBytes = new MemoryStream();
                }
            }

            lastByte = curByte;
        }

        private async Task ReadCurMenu()
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=curmenu");
            var result = Http.ReadResponse<MenuSetRuquestResult>(allmenuString);

            try
            {
                // MenuSet = MenuSet.TryParseMenuSet(result.MenuSet, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            }
            catch (AggregateException ex)
            {
                Log.Error(new Exception("Cannot parse MenuSet.\r\n" + allmenuString, ex));
            }

            await http.Get<BaseRequestResult>("?mode=getinfo&type=curmenu");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        private async Task ReadLensInfo()
        {
            var raw = await http.GetString("?mode=getinfo&type=lens");
            var newInfo = Parser.ParseLensInfo(raw);
            if (!Equals(newInfo, LensInfo))
            {
                LensInfo = newInfo;
                UpdateCurrentApertures();

                OnPropertyChanged(nameof(LensInfo));
                OnPropertyChanged(nameof(CurrentApertures));
            }
        }

        private async Task<bool> ReadMenuSet(string lang)
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=allmenu");
            var result = Http.ReadResponse<MenuSetRuquestResult>(allmenuString);

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
            catch (AggregateException ex)
            {
                Log.Error(new Exception("Cannot parse MenuSet.\r\n" + allmenuString, ex));
                return false;
            }
        }

        private async void StateTimer_Tick(object sender, object e)
        {
            await stateUpdatingSem.WaitAsync();
            {
                try
                {
                    if (IsConnected)
                    {
                        await UpdateState();
                    }
                }
                catch (Exception)
                {
                    await Disconnect();
                }
                finally
                {
                    stateUpdatingSem.Release();
                }
            }
        }

        private async Task TryReadLensInfo()
        {
            try
            {
                await ReadLensInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void UpdateCurrentApertures()
        {
            var open = OffFrameProcessor?.OpenedAperture ?? LensInfo.OpenedAperture;

            var newCurrentApertures = new List<CameraMenuItem256>(MenuSet.Apertures.Count);
            var opentext = CameraParser.ApertureBinToText(open);
            newCurrentApertures.Add(new CameraMenuItem256(open.ToString(), opentext, "setsetting", "shtrspeed", open));
            newCurrentApertures.AddRange(MenuSet.Apertures.Where(a => a.IntValue >= open && a.Text != opentext && a.IntValue <= LensInfo.ClosedAperture));
            if (newCurrentApertures.Count != CurrentApertures.Count ||
                newCurrentApertures.First().Value != CurrentApertures.First().Value)
            {
                CurrentApertures = newCurrentApertures;
                OnPropertyChanged(nameof(CurrentApertures));
            }
        }

        private async Task<CameraState> UpdateState()
        {
            var newState = await http.Get<CameraStateRequestResult>("?mode=getstate");
            if (State != null && newState.State.Equals(State))
            {
                return State;
            }

            State = newState.State ?? throw new NullReferenceException();

            RecState = State.Rec == OnOff.On ? RecState.Started : RecState.Stopped;

            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(RecState));
            return State;
        }
    }
}