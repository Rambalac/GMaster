namespace GMaster.Camera
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Annotations;
    using Rssdp;
    using Rssdp.Infrastructure;
    using Windows.Networking;
    using Windows.Networking.Connectivity;
    using Windows.Networking.Sockets;

    public class LumixManager
    {
        private const int LiveViewPort = 49152;

        private readonly string lang;
        private readonly ConcurrentDictionary<string, Lumix> listeners = new ConcurrentDictionary<string, Lumix>();
        private List<SsdpDeviceLocator> deviceLocators;
        private List<DatagramSocket> liveviewUdpSockets;

        public LumixManager(string lang)
        {
            this.lang = lang;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        public event Action<DeviceInfo> DeviceDiscovered;

        [NotNull]
        public async Task<Lumix> ConnectCamera(DeviceInfo device)
        {
            var result = new Lumix(device);
            if (!listeners.TryAdd(result.CameraHost, result))
            {
                throw new Exception("Should not be more than one listener for address");
            }

            result.Disconnected += Result_Disconnected;

            await result.Connect(LiveViewPort, lang);

            return result;
        }

        public async Task SearchCameras()
        {
            if (deviceLocators != null)
            {
                foreach (var dev in deviceLocators)
                {
                    await dev.SearchAsync();
                }
            }
        }

        public async Task StartListening()
        {
            foreach (var listener in listeners.Values)
            {
                listener.ManagerRestarted();
            }

            liveviewUdpSockets = new List<DatagramSocket>();
            foreach (var profile in NetworkInformation.GetHostNames())
            {
                var liveviewUdp = new DatagramSocket();
                try
                {
                    liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;

                    await liveviewUdp.BindEndpointAsync(profile, LiveViewPort.ToString());
                    liveviewUdpSockets.Add(liveviewUdp);
                }
                catch (Exception)
                {
                    liveviewUdp.Dispose();
                }
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

        public void StopListening()
        {
            if (liveviewUdpSockets != null)
            {
                foreach (var liveviewUdpSocket in liveviewUdpSockets)
                {
                    liveviewUdpSocket.Dispose();
                }
            }

            liveviewUdpSockets = null;

            if (deviceLocators != null)
            {
                foreach (var deviceLocator in deviceLocators)
                {
                    deviceLocator.StopListeningForNotifications();
                    deviceLocator.Dispose();
                }
            }

            deviceLocators = null;
        }

        protected void OnDeviceDiscovered(DeviceInfo obj)
        {
            DeviceDiscovered?.Invoke(obj);
        }

        private async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs arg)
        {
            try
            {
                if (!arg.IsNewlyDiscovered)
                {
                    return;
                }

                var info = await arg.DiscoveredDevice.GetDeviceInfo() as SsdpRootDevice;
                if (info == null)
                {
                    return;
                }

                if (info.ModelName != "LUMIX")
                {
                    return;
                }

                var dev = new DeviceInfo(info);
                OnDeviceDiscovered(dev);
            }
            catch (HttpRequestException e)
            {
                // var status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.WriteLine(e);

                // Ignore because GetDeviceInfo has problems
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Log.Error(e);
            }
        }

        private IEnumerable<string> GetLocalAddresses()
        {
            return NetworkInformation.GetHostNames()
                .Where(hn => (hn.Type == HostNameType.Ipv4 || hn.Type == HostNameType.Ipv6))
                .Select(h => h.CanonicalName);
        }

        private void LiveviewUdp_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                if (!listeners.TryGetValue(args.RemoteAddress.CanonicalName, out Lumix camera))
                {
                    return;
                }

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

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            StopListening();
            await StartListening();
        }

        private void Result_Disconnected(Lumix obj, bool stillAvailabe)
        {
            if (!listeners.TryRemove(obj.CameraHost, out _))
            {
                throw new Exception("Listener is not connected");
            }
        }
    }
}