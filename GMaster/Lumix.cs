using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using GMaster.Annotations;
using GMaster.LumixResponces;
using Serilog;
using UPnP;

namespace GMaster
{
    public class Lumix : INotifyPropertyChanged
    {
        private const int LiveViewPort = 49152;

        private static readonly XmlMediaTypeFormatter Formatter = new XmlMediaTypeFormatter {UseXmlSerializer = true};
        private static DatagramSocket liveviewUdp;

        private static readonly ConcurrentDictionary<string, Lumix> Listeners =
            new ConcurrentDictionary<string, Lumix>();

        private readonly HttpClient camcgi;
        private readonly string cameraHost;
        private readonly Device device;

        private readonly object messageRecieving = new object();

        private readonly DispatcherTimer stateTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(4)};

        private MemoryStream currentImageStream;

        private byte lastByte;

        /**
         * Create the Lumix videostream reader.
         * 
         * @param cameraIp IPv4 address of the camera.
         * @param cameraNetmaskBitSize Size of the camera network's subnet.
         * @param udpPort The UDP port to listen on.
         * 
         * @throws UnknownHostException If the camera IP address cannot be parsed.
         * @throws SocketException On network communication errors.
         */

        public Lumix(Device device)
        {
            this.device = device;
            cameraHost = new Uri(device.URLBase).Host;
            camcgi = new HttpClient {BaseAddress = new Uri($"http://{cameraHost}/cam.cgi")};
            camcgi.DefaultRequestHeaders.Accept.Clear();
            camcgi.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            stateTimer.Tick += StateTimer_Tick;
        }

        public bool IsConnected { get; private set; }

        public string Udn => device.UDN;

        public MemoryStream LiveViewFrame { get; private set; }

        public CameraState State { get; private set; }

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

        public static async Task<IEnumerable<Device>> SearchCameras()
        {
            var devices = await new Ssdp().SearchUPnPDevices("MediaServer");

            return devices.Where(d => d.ModelName == "LUMIX");
        }

        private async Task<T> Get<T>(string path) where T : BaseRequestResult
        {
            var response = await camcgi.GetAsync(path);
            if (!response.IsSuccessStatusCode) throw new LumixException("Request failed: " + path);
            var product = await response.Content.ReadAsAsync<T>(new[] {Formatter});
            if (product.Result != "ok")
                throw new LumixException(
                    $"Not ok result\r\nRequest: {path}\r\n{await response.Content.ReadAsStringAsync()}");
            return product;
        }

        private async Task<CameraState> UpdateState()
        {
            var newState = await Get<CameraStateRequestResult>("?mode=getstate");
            if (!(newState.State?.Equals(State) ?? State == null))
            {
                State = newState.State;
                OnPropertyChanged(nameof(State));
            }
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

                if (liveviewUdp == null)
                    await StartListening();
                await SwitchToRec();
                await UpdateState();
                stateTimer.Start();
                await Get<BaseRequestResult>($"?mode=startstream&value={LiveViewPort}");
                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
                if (!Listeners.TryAdd(cameraHost, this))
                    throw new Exception("Should not be more than one listener for address");
            }
            catch (Exception e)
            {
                Log.Error(e, "Connect");
                StopListening();
                throw;
            }
        }

        public async Task Disconnect()
        {
            StopListening();
            try
            {
                stateTimer.Stop();
                await Get<BaseRequestResult>("?mode=stopstream");
                Lumix camera;
                if (!Listeners.TryRemove(cameraHost, out camera)) throw new Exception("Listener is not connected");
                if (!ReferenceEquals(camera, this)) throw new Exception("Wrong listener");
            }
            catch (Exception)
            {
            }
            finally
            {
                IsConnected = false;
                Disconnected?.Invoke(this);
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public event Action<Lumix> Disconnected;

        private static async Task StartListening()
        {
            liveviewUdp = new DatagramSocket();
            liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;
            await liveviewUdp.BindServiceNameAsync(LiveViewPort.ToString());
        }

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
                    }
                }
            }
        }

        private static void LiveviewUdp_MessageReceived(DatagramSocket sender,
            DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                Lumix camera;
                if (!Listeners.TryGetValue(args.RemoteAddress.CanonicalName, out camera))
                    throw new NullReferenceException(@"Camera not found: {args.RemoteAddress.CanonicalName}");
                camera.ProcessMessage(args.GetDataReader());
            }
            catch (Exception e)
            {
                Log.Error(e, "UDP Message");
            }
        }

        private static void StopListening()
        {
            liveviewUdp?.Dispose();
            liveviewUdp = null;
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
            return obj.GetType() == GetType() && Equals((Lumix) obj);
        }

        public override int GetHashCode()
        {
            return Udn?.GetHashCode() ?? 0;
        }
    }
}