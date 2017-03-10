using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using GMaster.Annotations;
using UPnP;

namespace GMaster.Views
{
    public class MainPageModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer cameraRefreshTimer;


        public MainPageModel()
        {
            View1 = new CameraViewModel { GlobalModel = this };

            if (!DesignMode.DesignModeEnabled)
            {
                cameraRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
                cameraRefreshTimer.Start();
                Task.Run(CameraRefresh);

                wifidirect = new WiFiDirectHelper();
                wifidirect.Start();
            }
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public CameraViewModel View1 { get; private set; }

        private async Task CameraRefresh()
        {
            try
            {
                var newDevices = await Lumix.SearchCameras();
                var toRemove = Devices.Where(d1 => newDevices.All(d2 => d1.UDN != d2.UDN)).ToList();
                toRemove.RemoveAll(d1 => connectedDevices.ContainsKey(d1.UDN));

                var toAdd = newDevices.Where(d2 => Devices.All(d1 => d1.UDN != d2.UDN)).ToList();

                await App.RunAsync(() =>
                {
                    foreach (var device in toRemove)
                    {
                        Devices.Remove(device);
                    }
                    foreach (var device in toAdd)
                    {
                        Devices.Add(device);
                    }
                    OnPropertyChanged();
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "Camera refresh");
                throw;
            }
        }

        private readonly Dictionary<string, Lumix> connectedDevices = new Dictionary<string, Lumix>();
        private WiFiDirectHelper wifidirect;

        public event Action<Lumix> CameraDisconnected;

        private async void CameraRefreshTimer_Tick(object sender, object e)
        {
            await CameraRefresh();
        }

        public void AddConnectedDevice(Lumix lumix)
        {
            connectedDevices.Add(lumix.Udn, lumix);
            lumix.Disconnected += Lumix_Disconnected;
        }

        private void Lumix_Disconnected(Lumix lumix)
        {
            lumix.Disconnected -= Lumix_Disconnected;
            connectedDevices.Remove(lumix.Udn);
            OnCameraDisconnected(lumix);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool TryGetConnectedDevice(string udn, out Lumix connectedDevice)
        {
            return connectedDevices.TryGetValue(udn, out connectedDevice);
        }

        protected virtual void OnCameraDisconnected(Lumix obj)
        {
            CameraDisconnected?.Invoke(obj);
        }
    }
}
