using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using LumixMaster.Annotations;
using UPnP;

namespace LumixMaster
{
    public class UpnpDeviceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as Device)?.FriendlyName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class ImageMemoryStreamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stream = value as MemoryStream;
            if (stream == null) return null;

            stream.Position = 0;
            var bitmap = new BitmapImage();
            bitmap.SetSource(stream.AsRandomAccessStream());
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }


    public class MainPageModel : INotifyPropertyChanged
    {
        private DispatcherTimer cameraRefreshTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        private Device selectedDevice;
        private Lumix selectedCamera;

        public Visibility ConnectVisibility => (SelectedDevice != null && SelectedCamera == null) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DisconnectVisibility => (SelectedCamera != null) ? Visibility.Visible : Visibility.Collapsed;

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
                    {
                        SelectedCamera = connectedDevice;
                    }
                    else
                    {
                        SelectedCamera = null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

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

        private Dictionary<string, Lumix> ConnectedDevices { get; } = new Dictionary<string, Lumix>();

        public Lumix SelectedCamera
        {
            get { return selectedCamera; }
            set
            {
                selectedCamera = value;
                OnPropertyChanged(nameof(SelectedCamera));
            }
        }

        public MainPageModel()
        {
            cameraRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
            cameraRefreshTimer.Start();
            Task.Run(CameraRefresh);
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
                    {
                        Devices.Remove(device);
                    }
                    foreach (var device in toAdd)
                    {
                        Devices.Add(device);
                    }

                    if (SelectedDevice == null && Devices.Any()) SelectedDevice = Devices[0];
                });
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async void CameraRefreshTimer_Tick(object sender, object e)
        {
            await CameraRefresh();
        }

        [NotifyPropertyChangedInvocator]
        protected async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ConnectedDevicesContains(string selectedDeviceUdn)
        {
            return ConnectedDevices.ContainsKey(selectedDeviceUdn);
        }
    }
}
