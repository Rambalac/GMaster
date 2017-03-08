using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Graphics.Imaging;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using LumixMaster.Annotations;
using LumixMaster.LumixResponces;
using UPnP;


namespace LumixMaster
{
    /*
     * An improved version of the Panasonic Lumix camera desktop stream viewer. Based upon the work published at:  
     * http://www.personal-view.com/talks/discussion/6703/control-your-gh3-from-a-web-browser-now-with-video-/p1
     * 
     * version 2.0.0
     * 
     * author personal-view.com
     * author Martin Pecka
     * author Rambalac
     */
    public class Lumix : INotifyPropertyChanged
    {
        private Device device;
        private CancellationTokenSource listenerCancellationTokenSource;
        private Task listenerTask;
        private string cameraHost;
        private HttpClient camcgi;
        private const int liveViewPort = 49152;

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
            camcgi = new HttpClient { BaseAddress = new Uri($"http://{cameraHost}/cam.cgi") };
            camcgi.DefaultRequestHeaders.Accept.Clear();
            camcgi.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            stateTimer.Tick += StateTimer_Tick;
        }

        public bool IsConnected { get; private set; }

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

        public string Udn => device.UDN;

        public MemoryStream LiveViewFrame { get; private set; }

        private MemoryStream currentImageStream;

        public CameraState State { get; private set; }

        public static async Task<IEnumerable<Device>> SearchCameras()
        {
            var devices = await new Ssdp().SearchUPnPDevices("MediaServer");

            return devices.Where(d => d.ModelName == "LUMIX");
        }

        private static XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter { UseXmlSerializer = true };
        private DatagramSocket liveviewUdp;

        private async Task<T> Get<T>(string path) where T : BaseRequestResult
        {
            var response = await camcgi.GetAsync(path);
            if (!response.IsSuccessStatusCode) throw new LumixException("Request failed: " + path);
            var product = await response.Content.ReadAsAsync<T>(new[] { formatter });
            if (product.Result != "ok") throw new LumixException(
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

        private DispatcherTimer stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };

        public async Task SwitchToRec()
        {
            await Get<BaseRequestResult>("?mode=camcmd&value=recmode");
        }

        public async Task Connect()
        {
            try
            {
                await StartListening();
                await SwitchToRec();
                await UpdateState();
                stateTimer.Start();
                await Get<BaseRequestResult>($"?mode=startstream&value={liveViewPort}");
                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));

            }
            catch (Exception e)
            {
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

        private async Task StartListening()
        {
            liveviewUdp?.Dispose();
            currentImageStream = null;
            lastByte = 0;

            liveviewUdp = new DatagramSocket();
            liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;
            await liveviewUdp.BindServiceNameAsync(liveViewPort.ToString());
        }

        private readonly object messageRecieving = new object();

        private byte lastByte;

        private void LiveviewUdp_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                lock (messageRecieving)
                {
                    using (var reader = args.GetDataReader())
                    {
                        {
                            while (reader.UnconsumedBufferLength > 0)
                            {
                                var curByte = reader.ReadByte();
                                currentImageStream?.WriteByte(curByte);


                                if (lastByte == 0xff)
                                {
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

                                }

                                lastByte = curByte;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void StopListening()
        {
            liveviewUdp?.Dispose();
            liveviewUdp = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Lumix)obj);
        }

        public override int GetHashCode()
        {
            return (Udn != null ? Udn.GetHashCode() : 0);
        }
    }
}
