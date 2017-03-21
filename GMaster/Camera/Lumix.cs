using System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace GMaster.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Annotations;
    using LumixResponces;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using Windows.Web.Http.Headers;
    using HttpClient = Windows.Web.Http.HttpClient;
    using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

    public class Lumix : INotifyPropertyChanged
    {
        private readonly Uri baseUri;

        private readonly HttpClient camcgi;

        private readonly object messageRecieving = new object();

        private readonly DispatcherTimer stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        private MemoryStream currentImageStream;

        private byte lastByte;

        private AbstractMenuSetParser menuset;

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

        public DeviceInfo Device { get; }

        public bool IsConnected { get; private set; }

        public bool IsLimited => menuset == null;

        public MemoryStream LiveViewFrame { get; private set; }

        public RecState RecState { get; private set; } = RecState.Unknown;

        public CameraState State { get; private set; }

        public string Udn => Device.Udn;

        public async Task Capture()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=capture");
            await Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
        }

        public async Task Connect(int liveviewport, string lang)
        {
            try
            {
                currentImageStream = null;
                lastByte = 0;

                await ReadMenuSet(lang);
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

        //http://192.168.11.15/cam.cgi?mode=getinfo&type=curmenu

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

        public void ProcessMessage(ArraySegment<byte> buf)
        {
            lock (messageRecieving)
            {
                foreach (var b in buf)
                {
                    ProcessByte(b);
                }
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

        private static string PropertyString(object value)
        {
            switch (value)
            {
                case null:
                    return null;

                case Enum en:
                    var attribute = typeof(Enum)
                        .GetMember(en.ToString())
                        .First()
                        .GetCustomAttribute<XmlElementAttribute>();
                    return attribute?.ElementName ?? en.ToString();

                default:
                    return value.ToString();
            }
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
                    throw new LumixException($"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
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

        private async Task<TResponse> Post<TResponse>(string path, Dictionary<string, string> parameters)
            where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, path);
            var content = new HttpFormUrlEncodedContent(parameters);

            using (var response = await camcgi.PostAsync(uri, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new LumixException("Request failed: " + path);
                }

                var product = await ReadResponse<TResponse>(response);
                if (product.Result != "ok")
                {
                    throw new LumixException($"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
                }

                return product;
            }
        }

        private void ProcessByte(byte curByte)
        {
            currentImageStream?.WriteByte(curByte);

            if (lastByte == 0xff)
            {
                if (curByte == 0xd8)
                {
                    currentImageStream = new MemoryStream(32768);
                    currentImageStream.WriteByte(0xff);
                    currentImageStream.WriteByte(0xd8);
                }
                else if (currentImageStream != null && curByte == 0xd9)
                {
                    LiveViewFrame = currentImageStream;
                    App.RunAsync(() => OnPropertyChanged(nameof(LiveViewFrame)));

                    currentImageStream = null;
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
                menuset = AbstractMenuSetParser.TryParse(result.MenuSet, lang);
            }
            catch (AggregateException ex)
            {
                Log.Error(new Exception("Cannot parse MenuSet.\r\n" + allmenuString, ex));
            }
        }

        private async Task SetSetting<TRequest>(string settingName, TRequest settings)
        {
            var properties = settings.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => PropertyString(prop.GetValue(settings, null)));
            await Post<BaseRequestResult>("?mode=setsetting", properties);
        }

        private readonly SemaphoreSlim stateUpdatingSem = new SemaphoreSlim(1);

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