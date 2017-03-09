using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using GMaster.Annotations;
using Serilog;
using UPnP;

namespace GMaster.Views
{
    public class MainPageModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer cameraRefreshTimer;
        private Lumix selectedCamera;

        private Device selectedDevice;

        public MainPageModel()
        {
            cameraRefreshTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(5)};
            cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
            cameraRefreshTimer.Start();
            Task.Run(CameraRefresh);
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public bool IsConnectVisibile => SelectedDevice != null && SelectedCamera == null;

        public bool IsDisconnectVisibile => SelectedCamera != null;

        public Device SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                try
                {
                    selectedDevice = value;
                    OnPropertyChanged();

                    Lumix connectedDevice;
                    if (selectedDevice != null && ConnectedDevices.TryGetValue(selectedDevice.UDN, out connectedDevice))
                        SelectedCamera = connectedDevice;
                    else
                        SelectedCamera = null;
                    OnPropertyChanged(nameof(IsConnectVisibile));
                    OnPropertyChanged(nameof(IsDisconnectVisibile));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private Dictionary<string, Lumix> ConnectedDevices { get; } = new Dictionary<string, Lumix>();

        public Lumix SelectedCamera
        {
            get { return selectedCamera; }
            set
            {
                selectedCamera = value;
                OnPropertyChanged(nameof(SelectedCamera));
                OnPropertyChanged(nameof(IsConnectVisibile));
                OnPropertyChanged(nameof(IsDisconnectVisibile));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddConnectedDevice(Lumix lumix)
        {
            ConnectedDevices.Add(lumix.Udn, lumix);
            lumix.Disconnected += Lumix_Disconnected;
        }

        private void Lumix_Disconnected(Lumix lumix)
        {
            lumix.Disconnected -= Lumix_Disconnected;
            ConnectedDevices.Remove(lumix.Udn);
            if (lumix.Equals(SelectedCamera)) SelectedCamera = null;
        }

        private async Task CameraRefresh()
        {
            try
            {
                var newDevices = await Lumix.SearchCameras();
                var toRemove = Devices.Where(d1 => newDevices.All(d2 => d1.UDN != d2.UDN)).ToList();
                toRemove.RemoveAll(d1 => ConnectedDevices.ContainsKey(d1.UDN));

                var toAdd = newDevices.Where(d2 => Devices.All(d1 => d1.UDN != d2.UDN)).ToList();

                await App.RunAsync(() =>
                {
                    foreach (var device in toRemove)
                        Devices.Remove(device);
                    foreach (var device in toAdd)
                        Devices.Add(device);

                    if (SelectedDevice == null && Devices.Any()) SelectedDevice = Devices[0];
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "Camera refresh");
                throw;
            }
        }

        private async void CameraRefreshTimer_Tick(object sender, object e)
        {
            await CameraRefresh();
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ConnectedDevicesContains(string selectedDeviceUdn)
        {
            return ConnectedDevices.ContainsKey(selectedDeviceUdn);
        }
    }
}