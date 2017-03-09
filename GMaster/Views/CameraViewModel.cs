using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using GMaster.Annotations;
using UPnP;

namespace GMaster.Views
{
    public class CameraViewModel : INotifyPropertyChanged
    {
        public MainPageModel GlobalModel
        {
            get { return globalModel; }
            set
            {
                globalModel = value;
                globalModel.PropertyChanged += GlobalModel_PropertyChanged;
                globalModel.CameraDisconnected += GlobalModel_CameraDisconnected;
            }
        }

        private void GlobalModel_CameraDisconnected(Lumix lumix)
        {
            if (ReferenceEquals(lumix, SelectedCamera)) SelectedCamera = null;
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        private void GlobalModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GlobalModel.Devices))
            {
                if (SelectedDevice == null && GlobalModel.Devices.Any()) SelectedDevice = GlobalModel.Devices[0];
            }
        }

        private Lumix selectedCamera;

        private Device selectedDevice;
        private MainPageModel globalModel;

        public CameraViewModel()
        {
            DisconnectCommand = new DisconnectCommand(this);
            ConnectCommand = new ConnectCommand(this);
        }

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
                    if (selectedDevice != null && GlobalModel.TryGetConnectedDevice(selectedDevice.UDN, out connectedDevice))
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


        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}