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
    using Windows.Security.Credentials;
    using Windows.UI.Xaml.Controls;
    using System.Threading;
    using Tools;

    public class WiFiHelper : INotifyPropertyChanged
    {
        private WiFiAdapter adapter;

        private DispatcherTimer scanTimer;
        private DispatcherTimer connectedTimer;

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

        private async Task<PasswordCredential> AskPassword()
        {
            var cred = new PasswordCredential();

            var inputTextBox = new PasswordBox();
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = "WiFi Password";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Cancel";
            dialog.SecondaryButtonText = "Next";
            bool set = false;
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

        public async Task ConnecAccessPoint(WiFiAvailableNetwork ap)
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
                connectedTimer.Tick += ConnectedTimer_Tick; ;
                connectedTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async void ConnectedTimer_Tick(object sender, object e)
        {
            await CheckConnected();
        }

        private async Task CheckConnected()
        {
            var profile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
            if (ConnectedWiFi != profile?.ProfileName)
            {
                ConnectedWiFi = profile?.ProfileName;
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

        public bool autoconnectAlways = false;
        public bool AutoconnectAlways
        {
            get => autoconnectAlways; set
            {
                autoconnectAlways = value;
                OnPropertyChanged();
            }
        }

        private void Adapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            var list = sender.NetworkReport.AvailableNetworks.Where(n => !string.IsNullOrWhiteSpace(n.Ssid)).ToList();

            App.RunAsync(async () =>
            {
                var toadd = list.OrderByDescending(n => n.SignalBars).ToList();
                AccessPoints.Clear();

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

        int scanFlag = 0;

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