namespace GMaster.Views
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;
    using Camera;
    using Commands;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;

    public class MainPageModel : INotifyPropertyChanged, IDisposable
    {
        private readonly DispatcherTimer cameraRefreshTimer;
        private readonly SemaphoreSlim camerasearchSem = new SemaphoreSlim(1);
        private readonly WiFiHelper wifidirect;
        private bool leftPanelOpened;
        private DeviceInfo selectedDevice;

        public MainPageModel()
        {
            LumixManager = new LumixManager(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            LumixManager.DeviceDiscovered += Lumix_DeviceDiscovered;

            cameraRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
            cameraRefreshTimer.Start();
            Task.Run(CameraRefresh);

            wifidirect = new WiFiHelper();
            wifidirect.Start();

            ConnectCommand = new ConnectCommand(this);
            SelectCameraCommand = new SelectCameraCommand(this);
            DonateCommand = new DonateCommand(this);
        }

        public event Action<Lumix> CameraDisconnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DeviceInfo> ConnectableDevices { get; } = new ObservableCollection<DeviceInfo>();

        public ConnectCommand ConnectCommand { get; }

        public ObservableCollection<ConnectedCamera> ConnectedCameras { get; } = new ObservableCollection<ConnectedCamera>();

        public DonateCommand DonateCommand { get; }

        public Donations Donations { get; } = new Donations();

        public GeneralSettings GeneralSettings { get; } = new GeneralSettings();

        public bool IsMainMenuOpened
        {
            get => leftPanelOpened;

            set
            {
                leftPanelOpened = value;
                OnPropertyChanged();
            }
        }

        public LumixManager LumixManager { get; }

        public SelectCameraCommand SelectCameraCommand { get; }

        public DeviceInfo SelectedDevice
        {
            get => selectedDevice;

            set
            {
                selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public CameraViewModel View1 { get; } = new CameraViewModel();

        public string Version
        {
            get
            {
                var ver = Package.Current.Id.Version;
                return $"v{ver.Major}.{ver.Minor}.{ver.Build}";
            }
        }

        public void AddConnectableDevice(DeviceInfo device)
        {
            ConnectableDevices.Add(device);
            if (SelectedDevice == null)
            {
                SelectedDevice = device;
            }
        }

        public ConnectedCamera AddConnectedDevice(Lumix lumix)
        {
            ConnectableDevices.Remove(lumix.Device);
            if (!GeneralSettings.Cameras.TryGetValue(lumix.Udn, out var settings))
            {
                settings = new CameraSettings(lumix.Udn);
            }

            settings.GeneralSettings = GeneralSettings;

            var connectedCamera = new ConnectedCamera { Camera = lumix, Model = this, Settings = settings };
            ConnectedCameras.Add(connectedCamera);
            lumix.Disconnected += Lumix_Disconnected;
            return connectedCamera;
        }

        public async Task ConnectCamera(DeviceInfo modelSelectedDevice)
        {
            var lumix = await LumixManager.ConnectCamera(modelSelectedDevice);
            var connectedCamera = AddConnectedDevice(lumix);

            if (View1.SelectedCamera == null)
            {
                View1.SelectedCamera = connectedCamera;
            }
        }

        public void Dispose()
        {
            camerasearchSem.Dispose();
        }

        public async Task StartListening()
        {
            await LumixManager.StartListening();
        }

        public void StopListening()
        {
            LumixManager.StopListening();
        }

        protected virtual void OnCameraDisconnected(Lumix obj)
        {
            CameraDisconnected?.Invoke(obj);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task CameraRefresh()
        {
            if (!await camerasearchSem.WaitAsync(0))
            {
                return;
            }

            try
            {
                await LumixManager.SearchCameras();
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
            finally
            {
                camerasearchSem.Release();
            }
        }

        private async void CameraRefreshTimer_Tick(object sender, object e)
        {
            await CameraRefresh();
        }

        private async void Lumix_DeviceDiscovered(DeviceInfo dev)
        {
            try
            {
                await App.RunAsync(async () =>
                {
                    var camerafound = false;
                    var cameraauto = false;
                    if (GeneralSettings.Cameras.TryGetValue(dev.Udn, out var settings))
                    {
                        cameraauto = settings.Autoconnect;
                        camerafound = true;
                    }

                    if ((camerafound && cameraauto) || (!camerafound && GeneralSettings.Autoconnect))
                    {
                        try
                        {
                            await ConnectCamera(dev);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                    else
                    {
                        AddConnectableDevice(dev);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        private void Lumix_Disconnected(Lumix lumix, bool stillAvailbale)
        {
            lumix.Disconnected -= Lumix_Disconnected;
            ConnectedCameras.Remove(ConnectedCameras.Single(c => c.Udn == lumix.Udn));
            if (stillAvailbale)
            {
                AddConnectableDevice(lumix.Device);
            }

            OnCameraDisconnected(lumix);
        }
    }
}