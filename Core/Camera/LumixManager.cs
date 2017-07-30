namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Network;
    using Tools;
    using Rssdp;
    using Rssdp.Infrastructure;

    public class CamerasManager
    {
        private const int LiveViewPort = 49152;

        private readonly HashSet<string> foundDevices = new HashSet<string>();
        private readonly string lang;
        private readonly INetwork network;
        private readonly ConcurrentDictionary<string, ICamera> ipToCamera = new ConcurrentDictionary<string, ICamera>();
        private readonly ConcurrentDictionary<string, ICamera> usnToCamera = new ConcurrentDictionary<string, ICamera>();
        private List<SsdpDeviceLocator> deviceLocators;
        private List<IDatagramSocket> liveviewUdpSockets;

        public CamerasManager(string lang, INetwork network)
        {
            this.lang = lang;
            this.network = network;
            network.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        public event Action<DeviceInfo, ICamera> DeviceDiscovered2;

        public async Task<bool> ConnectCamera(ICamera camera)
        {
            try
            {
                var connectResult = await camera.Connect(LiveViewPort, lang);
                if (connectResult)
                {
                    ipToCamera[camera.CameraHost] = camera;
                    usnToCamera[camera.Device.Usn] = camera;
                    Debug.WriteLine("Add listener: " + camera.CameraHost, "UDP");
                }

                return connectResult;
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("Connection failed", ex));
                return false;
            }
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
            liveviewUdpSockets = new List<IDatagramSocket>();
            var confirmedHosts = new List<string>();
            foreach (var profile in network.GetHostNames())
            {
                var liveviewUdp = network.CreateDatagramSocket();
                try
                {
                    liveviewUdp.MessageReceived += LiveviewUdp_MessageReceived;

                    await liveviewUdp.Bind(profile, LiveViewPort);
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

        public void ForgetDiscovery(DeviceInfo dev)
        {
            var usn = dev.Usn;
            var host = dev.Host;
            lock (foundDevices)
            {
                foundDevices.Remove(usn + host);
            }
        }

        public void ForgetCamera(ICamera cam)
        {
            var usn = cam.Device.Usn;
            var host = cam.Device.Host;
            ipToCamera.TryRemove(host, out _);
            usnToCamera.TryRemove(usn, out _);
        }

        private async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs arg)
        {
            try
            {
                var usn = arg.DiscoveredDevice.Usn;
                var host = arg.DiscoveredDevice.DescriptionLocation.Host;
                lock (foundDevices)
                {
                    if (foundDevices.Contains(usn + host))
                    {
                        Debug.WriteLine("Discovered but already found: " + usn, "Discovery");
                        return;
                    }

                    Debug.WriteLine("Discovered new: " + usn, "Discovery");

                    foundDevices.Add(usn + host);
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

                var dev = new DeviceInfo(info, usn);
                Log.Trace("Discovered " + dev.ModelName, tags: "camera." + dev.ModelName);

                if (usnToCamera.TryGetValue(usn, out var oldcamera))
                {
                    DeviceDiscovered2?.Invoke(dev, oldcamera);
                }
                else
                {
                    DeviceDiscovered2?.Invoke(dev, null);
                }
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

        private void LiveviewUdp_MessageReceived(DatagramSocketMessage args)
        {
            try
            {
                if (!ipToCamera.TryGetValue(args.RemoteAddress, out ICamera camera))
                {
                    return;
                }

                Task.Run(() => camera.ProcessUdpMessage(args.Data));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async void NetworkInformation_NetworkStatusChanged()
        {
            StopListening();
            await StartListening();
        }
    }
}