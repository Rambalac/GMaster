using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using GMaster.Annotations;

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
                Lumix.DeviceDiscovered += Lumix_DeviceDiscovered;

                cameraRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
                cameraRefreshTimer.Start();
                Task.Run(CameraRefresh);

                wifidirect = new WiFiDirectHelper();
                wifidirect.Start();
            }
        }

        private async void Lumix_DeviceDiscovered(Device dev)
        {
            try
            {
                await App.RunAsync(() =>
                {
                    Devices.Add(dev);
                    OnPropertyChanged();
                });
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public CameraViewModel View1 { get; private set; }

        private async Task CameraRefresh()
        {
            try
            {
                await Lumix.SearchCameras();
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private readonly Dictionary<Device, Lumix> connectedCameras = new Dictionary<Device, Lumix>();
        private WiFiDirectHelper wifidirect;

        public event Action<Lumix> CameraDisconnected;

        private async void CameraRefreshTimer_Tick(object sender, object e)
        {
            await CameraRefresh();
        }

        public void AddConnectedDevice(Lumix lumix)
        {
            connectedCameras.Add(lumix.Device, lumix);
            lumix.Disconnected += Lumix_Disconnected;
        }

        private void Lumix_Disconnected(Lumix lumix)
        {
            lumix.Disconnected -= Lumix_Disconnected;
            connectedCameras.Remove(lumix.Device);
            OnCameraDisconnected(lumix);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool TryGetConnectedCamera(Device dev, out Lumix connectedDevice)
        {
            return connectedCameras.TryGetValue(dev, out connectedDevice);
        }

        protected virtual void OnCameraDisconnected(Lumix obj)
        {
            CameraDisconnected?.Invoke(obj);
        }
    }
}
