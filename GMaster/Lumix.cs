using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using GMaster.Annotations;
using GMaster.LumixResponces;
using Rssdp;
using Rssdp.Infrastructure;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

namespace GMaster
{
    public class Device
    {
        internal Device(SsdpRootDevice dev)
        {
            Udn = dev.Udn;
            Host = dev.Location.Host;
            FriendlyName = dev.FriendlyName;
        }

        public string Udn { get; }
        public string Host { get; }

        public string FriendlyName { get; }
    }

    public class Lumix : INotifyPropertyChanged
    {
        private const int LiveViewPort = 49152;

        private static List<DatagramSocket> liveviewUdpSockets;

        private static readonly ConcurrentDictionary<string, Lumix> Listeners =
            new ConcurrentDictionary<string, Lumix>();

        private static List<SsdpDeviceLocator> deviceLocators;
        private readonly Uri baseUri;

        private static readonly HttpClient Camcgi;

        private readonly object messageRecieving = new object();

        private readonly DispatcherTimer stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        private MemoryStream currentImageStream;

        private byte lastByte;

        static Lumix()
        {
            var rootFilter = new HttpBaseProtocolFilter();
            rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            rootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            Camcgi = new HttpClient(rootFilter);
            Camcgi.DefaultRequestHeaders.Accept.Clear();
            Camcgi.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/xml"));

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private static async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            StopListening();
            await StartListening();
        }

        public Lumix(Device device)
        {
            Device = device;
            baseUri = new Uri($"http://{CameraHost}/cam.cgi");
            stateTimer.Tick += StateTimer_Tick;
        }

        public string CameraHost => Device.Host;
        public Device Device { get; }

        public bool IsConnected { get; private set; }

        public string Udn => Device.Udn;

        public MemoryStream LiveViewFrame { get; private set; }

        public CameraState State { get; private set; }

        public RecState RecState { get; private set; } = RecState.Unknown;

        public event PropertyChangedEventHandler PropertyChanged;

        private async void StateTimer_Tick(object sender, object e)
        {
            try
            {
                await UpdateState();
            }
            catch (Exception)
            {
                await Disconnect();
            }
        }

        public static async Task SearchCameras()
        {
            foreach (var dev in deviceLocators)
            {
                await dev.SearchAsync();
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

        private async Task<TResponse> Get<TResponse>(string path) where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, path);
            using (var response = await Camcgi.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode) throw new LumixException("Request failed: " + path);
                var product = await ReadResponse<TResponse>(response);
                if (product.Result != "ok")
                    throw new LumixException(
                        $"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
                return product;
            }
        }

        private async Task SetSetting<TRequest>(string settingName, TRequest settings)
        {
            var properties = settings.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => PropertyString(prop.GetValue(settings, null)));
            await Post<BaseRequestResult>("?mode=setsetting", properties);
        }

        private static string PropertyString(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Enum en:
                    var attribute = value.GetType()
                        .GetMember(value.ToString())
                        .First()
                        .GetCustomAttribute<XmlElementAttribute>();
                    return attribute?.ElementName ?? value.ToString();
                default:
                    return value.ToString();
            }
        }

        private async Task<TResponse> Post<TResponse>(string path, Dictionary<string, string> parameters)
            where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, path);
            var content = new HttpFormUrlEncodedContent(parameters);

            using (var response = await Camcgi.PostAsync(uri, content))
            {
                if (!response.IsSuccessStatusCode) throw new LumixException("Request failed: " + path);
                var product = await ReadResponse<TResponse>(response);
                if (product.Result != "ok")
                    throw new LumixException(
                        $"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
                return product;
            }
        }

        private async Task<CameraState> UpdateState()
        {
            var newState = await Get<CameraStateRequestResult>("?mode=getstate");
            if (State != null && newState.State.Equals(State)) return State;

            State = newState.State;

            if (State == null) throw new NullReferenceException();

            RecState = State.Rec == OnOff.On ? RecState.Started : RecState.Stopped;

            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(RecState));
            return State;
        }

        public async Task SwitchToRec()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=recmode");
        }

        public async Task Connect()
        {
            try
            {
                currentImageStream = null;
                lastByte = 0;

                await SwitchToRec();
                await UpdateState();
                stateTimer.Start();
                await Get<BaseRequestResult>($"?mode=startstream&value={LiveViewPort}");
                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
                if (!Listeners.TryAdd(CameraHost, this))
                    throw new Exception("Should not be more than one listener for address");
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
                stateTimer.Stop();
                try
                {
                    await Get<BaseRequestResult>("?mode=stopstream");
                }
                catch (Exception)
                {
                    //Ignore
                }

                if (!Listeners.TryRemove(CameraHost, out var camera)) throw new Exception("Listener is not connected");
                if (!ReferenceEquals(camera, this)) throw new Exception("Wrong listener");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                IsConnected = false;
                Disconnected?.Invoke(this);
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public event Action<Lumix> Disconnected;

        private static IEnumerable<string> GetLocalAddresses()
        {
            var cp = NetworkInformation.GetInternetConnectionProfile();
            return NetworkInformation.GetHostNames()
                .Where(hn => (hn.Type == HostNameType.Ipv4 || hn.Type == HostNameType.Ipv6) &&
                             hn.IPInformation.NetworkAdapter.NetworkAdapterId == cp.NetworkAdapter.NetworkAdapterId)
                .Select(h => h.CanonicalName);
        }

        public static async Task StartListening()
        {
            foreach (var listener in Listeners.Values)
            {
                lock (listener.messageRecieving)
                {
                    listener.currentImageStream = null;
                    listener.lastByte = 0;
                }
            }

            liveviewUdpSockets = new List<DatagramSocket>();
            foreach (var profile in GetLocalAddresses())
            {
                var liveviewUdp = new DatagramSocket();
                liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;
                await liveviewUdp.BindEndpointAsync(new HostName(profile), LiveViewPort.ToString());

                liveviewUdpSockets.Add(liveviewUdp);
            }

            deviceLocators = new List<SsdpDeviceLocator>();

            foreach (var host in GetLocalAddresses())
            {
                var deviceLocator =
                    new SsdpDeviceLocator(new SsdpCommunicationsServer(new SocketFactory(host)))
                    {
                        NotificationFilter = "urn:schemas-upnp-org:device:MediaServer:1"
                    };
                deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
                deviceLocator.StartListeningForNotifications();
                deviceLocators.Add(deviceLocator);
            }

        }

        private static async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs arg)
        {
            try
            {
                if (!arg.IsNewlyDiscovered) return;

                var info = await arg.DiscoveredDevice.GetDeviceInfo() as SsdpRootDevice;
                if (info == null) return;
                if (info.ModelName != "LUMIX") return;

                var dev = new Device(info);
                OnDeviceDiscovered(dev);
            }
            catch (HttpRequestException)
            {
                //Workaround
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static event Action<Device> DeviceDiscovered;


        private void ProcessMessage(DataReader reader)
        {
            lock (messageRecieving)
            {
                using (reader)
                {
                    {
                        while (reader.UnconsumedBufferLength > 0)
                        {
                            var curByte = reader.ReadByte();
                            ProcessByte(curByte);
                        }
                    }
                }
            }
        }

        private void ProcessByte(byte curByte)
        {
            currentImageStream?.WriteByte(curByte);

            if (lastByte == 0xff)
                if (currentImageStream == null && curByte == 0xd8)
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

            lastByte = curByte;
        }

        private static void LiveviewUdp_MessageReceived(DatagramSocket sender,
            DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                if (!Listeners.TryGetValue(args.RemoteAddress.CanonicalName, out Lumix camera))
                    return;
                using (var reader = args.GetDataReader())
                {
                    camera.ProcessMessage(reader);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async Task RecStart()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=video_recstart");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        public async Task Capture()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=capture");
            await Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
        }

        public async Task RecStop()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
            RecState = RecState.Unknown;
            OnPropertyChanged(nameof(RecState));
        }

        public static void StopListening()
        {
            foreach (var liveviewUdpSocket in liveviewUdpSockets)
            {
                liveviewUdpSocket.Dispose();
            }
            liveviewUdpSockets = null;

            foreach (var deviceLocator in deviceLocators)
            {
                deviceLocator.StopListeningForNotifications();
                deviceLocator.Dispose();
            }

            deviceLocators = null;
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

        protected bool Equals(Lumix other)
        {
            return Equals(Udn, other.Udn);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Lumix)obj);
        }

        public override int GetHashCode()
        {
            return Udn?.GetHashCode() ?? 0;
        }

        protected static void OnDeviceDiscovered(Device obj)
        {
            DeviceDiscovered?.Invoke(obj);
        }
    }

    public enum RecState
    {
        Stopped,
        Unknown,
        Started
    }
}