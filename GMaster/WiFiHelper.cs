namespace GMaster
{
    using Annotations;
    using Logger;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Windows.Devices.WiFi;
    using Windows.UI.Xaml;
    using Nito.AsyncEx;

    public class WiFiHelper : INotifyPropertyChanged
    {
        private WiFiAdapter adapter;

        private DispatcherTimer scanTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WiFiAvailableNetwork> AccessPoints { get; } = new ObservableCollection<WiFiAvailableNetwork>();

        public ObservableCollection<string> AutoconnectAccessPoints { get; } = new ObservableCollection<string>();

        public void MakeCurrentAutoconnect()
        {
            if (!AutoconnectAccessPoints.Contains(ConnectedWiFi))
            {
                AutoconnectAccessPoints.Add(ConnectedWiFi);
            }
        }

        private string connectedWiFi;

        public string ConnectedWiFi
        {
            get => connectedWiFi; private set
            {
                if (connectedWiFi != value)
                {
                    connectedWiFi = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Present { get; private set; }

        public async Task ConnecAccessPoint(WiFiAvailableNetwork ap)
        {
            var result = await adapter.ConnectAsync(ap, WiFiReconnectionKind.Automatic);
            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                ConnectedWiFi = ap.Ssid;
            }
        }

        public async Task Init()
        {
            try
            {
                if (await WiFiAdapter.RequestAccessAsync() != WiFiAccessStatus.Allowed)
                {
                    return;
                }

                var adapters = await WiFiAdapter.FindAllAdaptersAsync();
                if (adapters.Count > 1)
                {
                    Log.Warn("More than 1 WiFi adapter");
                }

                adapter = adapters.FirstOrDefault();
                if (adapter == null)
                {
                    return;
                }

                Present = true;
                OnPropertyChanged(nameof(Present));

                adapter.AvailableNetworksChanged += Adapter_AvailableNetworksChanged;

                await CheckAndScan();

                scanTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                scanTimer.Tick += ScanTimer_Tick;
                scanTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async Task CheckAndScan()
        {
            var profile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
            if (ConnectedWiFi != profile?.ProfileName)
            {
                ConnectedWiFi = profile?.ProfileName;
            }

            await adapter.ScanAsync();
        }

        public void Start()
        {
            scanTimer.Start();
        }

        public void Stop()
        {
            scanTimer.Stop();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool AutoconnectAlways { get; set; } = false;

        private void Adapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            var list = sender.NetworkReport.AvailableNetworks.Where(n => !string.IsNullOrWhiteSpace(n.Ssid)).ToList();

            App.RunAsync(async () =>
            {
                var toremove = AccessPoints.Except(list).ToList();
                var toadd = list.Except(AccessPoints).ToList();
                foreach (var ap in toremove)
                {
                    AccessPoints.Remove(ap);
                }

                foreach (var ap in toadd)
                {
                    AccessPoints.Add(ap);
                }

                if (AutoconnectAlways || ConnectedWiFi == null)
                {
                    foreach (string ssid in AutoconnectAccessPoints)
                    {
                        if (ssid == ConnectedWiFi)
                        {
                            break;
                        }

                        var autoconnect = list.FirstOrDefault(n => n.Ssid == ssid);
                        if (autoconnect != null)
                        {
                            var result = await adapter.ConnectAsync(autoconnect, WiFiReconnectionKind.Automatic);
                            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                            {
                                ConnectedWiFi = autoconnect.Ssid;
                            }

                            break;
                        }
                    }
                }
            });

        }

        bool scanFlag = false;

        private async void ScanTimer_Tick(object sender, object e)
        {
            if (!scanFlag)
            {
                try
                {
                    await CheckAndScan();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    scanFlag = false;
                }
            }
        }
    }
}