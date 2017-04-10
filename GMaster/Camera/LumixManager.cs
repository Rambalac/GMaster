namespace GMaster.Camera
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Annotations;
    using Logger;
    using Rssdp;
    using Rssdp.Infrastructure;
    using Tools;
    using Windows.Networking;
    using Windows.Networking.Connectivity;
    using Windows.Networking.Sockets;

    public class LumixManager
    {
        private const int LiveViewPort = 49152;

        private readonly HashSet<string> foundDevices = new HashSet<string>();
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

            result.Disconnected += Camera_Disconnected;

            if (await result.Connect(LiveViewPort, lang))
            {
                listeners[result.CameraHost] = result;
                return result;
            }

            return null;
        }

        public void SearchCameras()
        {
            try
            {
                if (deviceLocators != null)
                {
                    foreach (var dev in deviceLocators)
                    {
                        if (!dev.IsSearching)
                        {
                            var task = Task.Run(async () =>
                              {
                                  try
                                  {
                                      await dev.SearchAsync();
                                  }
                                  catch (ObjectDisposedException)
                                  {
                                      // Ignore due RSSDP lacks Cancellation
                                  }
                                  catch (Exception ex)
                                  {
                                      Log.Error(ex);
                                  }
                              });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public async Task StartListening()
        {
            liveviewUdpSockets = new List<DatagramSocket>();
            var confirmedHosts = new List<HostName>();
            foreach (var profile in NetworkInformation.GetHostNames().Where(h => h.Type == HostNameType.Ipv4 || h.Type == HostNameType.Ipv6))
            {
                var liveviewUdp = new DatagramSocket();
                try
                {
                    liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;

                    await liveviewUdp.BindEndpointAsync(profile, LiveViewPort.ToString());
                    liveviewUdpSockets.Add(liveviewUdp);
                    confirmedHosts.Add(profile);
                }
                catch (Exception)
                {
                    liveviewUdp.Dispose();
                }
            }

            lock (foundDevices)
            {
                foundDevices.Clear();
            }

            deviceLocators = new List<SsdpDeviceLocator>();

            foreach (var host in confirmedHosts)
            {
                var deviceLocator =
                    new SsdpDeviceLocator(new SsdpCommunicationsServer(new SocketFactory(host.CanonicalName)))
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

        private void Camera_Disconnected(Lumix obj, bool stillAvailabe)
        {
            if (!stillAvailabe)
            {
                lock (foundDevices)
                {
                    foundDevices.Clear();
                }
            }

            if (!listeners.TryRemove(obj.CameraHost, out _))
            {
                throw new Exception("Listener is not connected");
            }
        }

        private async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs arg)
        {
            try
            {
                lock (foundDevices)
                {
                    if (foundDevices.Contains(arg.DiscoveredDevice.Usn))
                    {
                        return;
                    }

                    foundDevices.Add(arg.DiscoveredDevice.Usn);
                }

                if (!arg.DiscoveredDevice.ResponseHeaders.TryGetValues("SERVER", out var values) || !values.Any(s => s.Contains("Panasonic")))
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
                Log.Error(e);
            }
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
                    var buf = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(buf);
                    camera.ProcessMessage(buf);
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
    }
}