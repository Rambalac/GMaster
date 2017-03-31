namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Annotations;
    using LumixResponces;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using Windows.Web.Http.Headers;

    public class Lumix : INotifyPropertyChanged
    {
        private static readonly HashSet<CameraMode> ApertureModes = new HashSet<CameraMode> { CameraMode.M, CameraMode.A, CameraMode.vM, CameraMode.vA };
        private static readonly HashSet<CameraMode> ShutterModes = new HashSet<CameraMode> { CameraMode.M, CameraMode.S, CameraMode.vM, CameraMode.vS };
        private readonly Uri baseUri;

        private readonly HttpClient camcgi;

        private readonly object messageRecieving = new object();

        private readonly DispatcherTimer stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        private readonly SemaphoreSlim stateUpdatingSem = new SemaphoreSlim(1);

        private MemoryStream currentImageStream;

        private byte lastByte;

        private MemoryStream offframeBytes;

        internal Lumix(DeviceInfo device)
        {
            Device = device;
            baseUri = new Uri($"http://{CameraHost}/cam.cgi");
            stateTimer.Tick += StateTimer_Tick;

            var rootFilter = new HttpBaseProtocolFilter();
            rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            rootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            camcgi = new HttpClient(rootFilter);
            camcgi.DefaultRequestHeaders.Accept.Clear();
            camcgi.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/xml"));
        }

        public delegate void DisconnectedDelegate(Lumix camera, bool stillAvailable);

        public event DisconnectedDelegate Disconnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CameraHost => Device.Host;

        public bool CanChangeAperture { get; private set; } = true;

        public bool CanChangeShutter { get; private set; } = true;

        public DeviceInfo Device { get; }

        public bool IsConnected { get; private set; }

        public bool IsLimited => MenuSet == null;

        public Stream LiveViewFrame { get; private set; }

        public MenuSet MenuSet { get; private set; }

        public OffframeProcessor OfframeProcessor { get; private set; }
        public CameraParser Parser { get; private set; }
        public RecState RecState { get; private set; } = RecState.Unknown;

        public CameraState State { get; private set; }

        public string Udn => Device.Udn;
        public async Task Capture()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=capture");
            await Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
        }

        public async Task ChangeFocus(FocusDirection focus)
        {
            await Get<BaseRequestResult>("?mode=camctrl&type=focus&value=" + focus.GetString());
        }

        public async Task Connect(int liveviewport, string lang)
        {
            try
            {
                currentImageStream = null;
                lastByte = 0;

                await ReadMenuSet(lang);

                OfframeProcessor = new OffframeProcessor(Device.ModelName, Parser);
                OfframeProcessor.PropertyChanged += OfframeProcessor_PropertyChanged;

                await SwitchToRec();
                await UpdateState();
                stateTimer.Start();
                await Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}");

                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
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
                    await Get<BaseRequestResult>("?mode=stopstream");
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

        public CameraMenuItem GetCurrentAperture()
        {
            return MenuSet.Apertures.FirstOrDefault(
                s => s.Value == OfframeProcessor.Aperture.Bin + "/256");
        }

        public CameraMenuItem GetCurrentIso()
        {
            return MenuSet.IsoValues.FirstOrDefault(
                s => s.Value == OfframeProcessor.Iso.Text);
        }

        public CameraMenuItem GetCurrentShutter()
        {
            return MenuSet.ShutterSpeeds.FirstOrDefault(
                s => s.Value == OfframeProcessor.Shutter.Bin + "/256");
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

        public void ProcessMessage(DataReader reader)
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

        public async Task ReadCurMenu()
        {
            var allmenuString = await GetString("?mode=getinfo&type=curmenu");
            var result = ReadResponse<MenuSetRuquestResult>(allmenuString);

            try
            {
                //MenuSet = MenuSet.TryParseMenuSet(result.MenuSet, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            }
            catch (AggregateException ex)
            {
                Log.Error(new Exception("Cannot parse MenuSet.\r\n" + allmenuString, ex));
            }

            await Get<BaseRequestResult>("?mode=getinfo&type=curmenu");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        public async Task RecStart()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=video_recstart");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        public async Task RecStop()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        public async Task SendMenuItem(CameraMenuItem value)
        {
            await Get<BaseRequestResult>(new Dictionary<string, string>
            {
                { "mode", value.Command },
                { "type", value.CommandType },
                { "value", value.Value }
            });
        }

        public async Task SwitchToRec()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=recmode");
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

        private static async Task<TResponse> ReadResponse<TResponse>(HttpResponseMessage response)
        {
            using (var content = response.Content)
            using (var str = await content.ReadAsInputStreamAsync())
            {
                var serializer = new XmlSerializer(typeof(TResponse));
                return (TResponse)serializer.Deserialize(str.AsStreamForRead());
            }
        }

        private static TResponse ReadResponse<TResponse>(string str)
        {
            var serializer = new XmlSerializer(typeof(TResponse));
            return (TResponse)serializer.Deserialize(new StringReader(str));
        }

        private async Task<TResponse> Get<TResponse>(string path)
            where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, path);
            using (var response = await camcgi.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new LumixException("Request failed: " + path);
                }

                var product = await ReadResponse<TResponse>(response);
                if (product.Result != "ok")
                {
                    throw new LumixException(
                        $"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
                }

                return product;
            }
        }

        private async Task<TResponse> Get<TResponse>(Dictionary<string, string> parameters)
            where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value)));
            using (var response = await camcgi.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new LumixException("Request failed: ");
                }

                var product = await ReadResponse<TResponse>(response);
                if (product.Result != "ok")
                {
                    throw new LumixException(
                        $"Not ok result\r\nRequest: \r\n{await response.Content.ReadAsStringAsync()}");
                }

                return product;
            }
        }

        private async Task<string> GetString(string path)
        {
            var uri = new Uri(baseUri, path);
            using (var response = await camcgi.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new LumixException("Request failed: " + path);
                }

                return await response.Content.ReadAsStringAsync();
            }
        }
        private void OfframeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OfframeProcessor.CameraMode))
            {
                CanChangeAperture = ApertureModes.Contains(OfframeProcessor.CameraMode);
                CanChangeShutter = ShutterModes.Contains(OfframeProcessor.CameraMode);
                OnPropertyChanged(nameof(CanChangeAperture));
                OnPropertyChanged(nameof(CanChangeShutter));
            }
        }

        private void ProcessByte(byte curByte)
        {
            currentImageStream?.WriteByte(curByte);

            offframeBytes?.WriteByte(curByte);

            if (lastByte == 0xff)
            {
                if (curByte == 0xd8)
                {
                    OfframeProcessor.Process(offframeBytes);
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

        private async Task ReadMenuSet(string lang)
        {
            var allmenuString = await GetString("?mode=getinfo&type=allmenu");
            var result = ReadResponse<MenuSetRuquestResult>(allmenuString);

            try
            {
                Parser = CameraParser.TryParseMenuSet(result.MenuSet, lang, out var menuset);
                MenuSet = menuset;
            }
            catch (AggregateException ex)
            {
                Log.Error(new Exception("Cannot parse MenuSet.\r\n" + allmenuString, ex));
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

        private async Task<CameraState> UpdateState()
        {
            var newState = await Get<CameraStateRequestResult>("?mode=getstate");
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