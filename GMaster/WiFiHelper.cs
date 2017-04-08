namespace GMaster
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Logger;
    using Windows.Devices.WiFi;
    using Windows.UI.Xaml;

    public class WiFiHelper : INotifyPropertyChanged
    {
        private WiFiAdapter adapter;

        private DispatcherTimer scanTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> AccessPoints { get; } = new ObservableCollection<string>();

        public bool Present { get; private set; }

        public async Task Init()
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

            await adapter.ScanAsync();

            scanTimer.Tick += ScanTimer_Tick;
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

        private void Adapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            var list = sender.NetworkReport.AvailableNetworks.Select(n => n.Ssid).ToList();

            var toremove = AccessPoints.Except(list);
            var toadd = list.Except(AccessPoints);
            foreach (var ap in toremove)
            {
                AccessPoints.Remove(ap);
            }
            foreach (var ap in toadd)
            {
                AccessPoints.Add(ap);
            }
        }

        private async void ScanTimer_Tick(object sender, object e)
        {
            await adapter.ScanAsync();
        }
    }
}