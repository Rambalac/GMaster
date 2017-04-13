namespace GMaster
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;
    using Core.Tools;
    using Windows.Devices.WiFi;
    using Windows.Security.Credentials;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Debug = System.Diagnostics.Debug;

    public class WiFiHelper : INotifyPropertyChanged
    {
        private WiFiAdapter adapter;
        private bool autoconnectAlways;
        private DispatcherTimer connectedTimer;
        private string connectedWiFi;
        private int scanFlag;
        private DispatcherTimer scanTimer;

        public event Action<IList<WiFiAvailableNetwork>> AccessPointsUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public ConcurrentBag<string> AutoconnectAccessPoints { get; } = new ConcurrentBag<string>();

        public bool AutoconnectAlways
        {
            get => autoconnectAlways; set
            {
                autoconnectAlways = value;
                OnPropertyChanged();
            }
        }

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

        public async Task ConnectAccessPoint(WiFiAvailableNetwork ap)
        {
            var result = await adapter.ConnectAsync(ap, WiFiReconnectionKind.Automatic);
            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                ConnectedWiFi = ap.Ssid;
                return;
            }

            if (result.ConnectionStatus != WiFiConnectionStatus.InvalidCredential)
            {
                return;
            }

            var cred = await AskPassword();

            result = await adapter.ConnectAsync(ap, WiFiReconnectionKind.Automatic, cred);
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

                await CheckConnected();
                await adapter.ScanAsync();

                scanTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
                scanTimer.Tick += ScanTimer_Tick;
                scanTimer.Start();

                connectedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                connectedTimer.Tick += ConnectedTimer_Tick;
                connectedTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void MakeCurrentAutoconnect()
        {
            if (!AutoconnectAccessPoints.Contains(ConnectedWiFi))
            {
                AutoconnectAccessPoints.Add(ConnectedWiFi);
            }
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

        private async void Adapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            var list = sender.NetworkReport.AvailableNetworks.Where(n => !string.IsNullOrWhiteSpace(n.Ssid)).OrderByDescending(n => n.SignalBars).ToList();

            AccessPointsUpdated?.Invoke(list);

            if (AutoconnectAlways || ConnectedWiFi == null)
            {
                foreach (var ssid in AutoconnectAccessPoints)
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
        }

        private async Task<PasswordCredential> AskPassword()
        {
            var cred = new PasswordCredential();

            var inputTextBox = new PasswordBox { Height = 32 };
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = "WiFi Password",
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Cancel",
                SecondaryButtonText = "Next"
            };
            var set = false;
            inputTextBox.KeyDown += (sender, arg) =>
            {
                if (arg.Key == Windows.System.VirtualKey.Enter)
                {
                    set = true;
                    dialog.Hide();
                }
            };

            var result = await dialog.ShowAsync();
            if (!set && result != ContentDialogResult.Secondary)
            {
                return null;
            }

            cred.Password = inputTextBox.Password;
            return cred;
        }

        private async Task CheckConnected()
        {
            var profile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
            if (ConnectedWiFi != profile?.ProfileName)
            {
                ConnectedWiFi = profile?.ProfileName;
            }
        }

        private async void ConnectedTimer_Tick(object sender, object e)
        {
            await CheckConnected();
        }

        private async void ScanTimer_Tick(object sender, object e)
        {
            if (Interlocked.CompareExchange(ref scanFlag, 1, 0) == 0)
            {
                try
                {
                    await adapter.ScanAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString(), "WiFiScan");
                }
                finally
                {
                    scanFlag = 0;
                }
            }
        }
    }
}