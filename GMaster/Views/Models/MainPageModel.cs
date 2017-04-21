namespace GMaster.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using Core.Camera;
    using Core.Tools;
    using Windows.ApplicationModel;
    using Windows.Devices.WiFi;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    public class MainPageModel : INotifyPropertyChanged
    {
        private readonly CameraViewModel[] allViews =
            { new CameraViewModel(), new CameraViewModel(), new CameraViewModel(), new CameraViewModel() };

        private readonly WiFiHelper wifi = new WiFiHelper();
        private CameraViewModel[] activeViews;
        private bool? isLandscape;
        private GridLength secondColumnWidth = new GridLength(0, GridUnitType.Star);
        private GridLength secondRowHeight = new GridLength(0);
        private DeviceInfo selectedDevice;
        private SplitMode splitMode;

        public MainPageModel()
        {
            wifi.AutoconnectAlways = GeneralSettings.WiFiAutoconnectAlways;
            foreach (var ap in GeneralSettings.WiFiAutoconnectAccessPoints.Value)
            {
                wifi.AutoconnectAccessPoints.Add(ap);
            }

            wifi.AutoconnectAlways = GeneralSettings.WiFiAutoconnectAlways;

            wifi.PropertyChanged += Wifi_PropertyChanged;
            wifi.AccessPointsUpdated += Wifi_AccessPointsUpdated;
            WifiAutoconnectAccessPoints.CollectionChanged += WifiAutoconnectAccessPoints_CollectionChanged;

            WifiAutoconnectAccessPoints.AddRange(GeneralSettings.WiFiAutoconnectAccessPoints.Value);

            ConnectionsManager = new ConnectionsManager(this, CultureInfo.CurrentUICulture);
        }

        public event Action<Lumix> CameraDisconnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public CameraViewModel[] ActiveViews
        {
            get => activeViews ?? (activeViews = new[] { View1 });

            set
            {
                activeViews = value;

                foreach (var view in allViews.Except(value))
                {
                    view.SelectedCamera = null;
                }
            }
        }

        public ObservableCollection<DeviceInfo> ConnectableDevices { get; } = new ObservableCollection<DeviceInfo>();

        public ObservableCollection<ConnectedCamera> ConnectedCameras { get; } =
            new ObservableCollection<ConnectedCamera>();

        public string ConnectedWiFi => wifi.ConnectedWiFi;

        public ConnectionsManager ConnectionsManager { get; }

        public CoreDispatcher Dispatcher { get; set; }

        public Donations Donations { get; } = new Donations();

        public GeneralSettings GeneralSettings { get; } = new GeneralSettings();

        public ObservableCollection<LutInfo> InstalledLuts { get; } = new ObservableCollection<LutInfo>();

        public bool IsDebug => Debugger.IsAttached;

        public bool IsLandscape
        {
            set
            {
                if (isLandscape != value)
                {
                    isLandscape = value;
                    SplitMode = value ? GeneralSettings.LandscapeSplitMode : GeneralSettings.PortraitSplitMode;
                }
            }
        }

        public GridLength SecondColumnWidth
        {
            get => secondColumnWidth;
            set
            {
                if (value.Equals(secondColumnWidth))
                {
                    return;
                }

                secondColumnWidth = value;
                OnPropertyChanged();
            }
        }

        public GridLength SecondRowHeight
        {
            get => secondRowHeight;
            set
            {
                if (value.Equals(secondRowHeight))
                {
                    return;
                }

                secondRowHeight = value;
                OnPropertyChanged();
            }
        }

        public DeviceInfo SelectedDevice
        {
            get => selectedDevice;

            set
            {
                selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public SplitMode SplitMode
        {
            get => splitMode;
            set
            {
                splitMode = value;
                switch (value)
                {
                    case SplitMode.One:
                        ActiveViews = new[] { View1 };
                        SecondColumnWidth = new GridLength(0);
                        SecondRowHeight = new GridLength(0);
                        if (View1.SelectedCamera == null)
                        {
                            if (View2.SelectedCamera != null)
                            {
                                View1.SelectedCamera = View2.SelectedCamera;
                                View2.SelectedCamera = null;
                            }
                            else if (View3.SelectedCamera != null)
                            {
                                View1.SelectedCamera = View3.SelectedCamera;
                                View3.SelectedCamera = null;
                            }
                        }

                        FillViews();
                        break;

                    case SplitMode.Horizontal:
                        ActiveViews = new[] { View1, View3 };
                        SecondColumnWidth = new GridLength(0);
                        SecondRowHeight = new GridLength(1, GridUnitType.Star);
                        if (View3.SelectedCamera == null && View2.SelectedCamera != null)
                        {
                            View3.SelectedCamera = View2.SelectedCamera;
                            View2.SelectedCamera = null;
                        }

                        FillViews();
                        break;

                    case SplitMode.Vertical:
                        ActiveViews = new[] { View1, View2 };
                        SecondColumnWidth = new GridLength(1, GridUnitType.Star);
                        SecondRowHeight = new GridLength(0);
                        if (View2.SelectedCamera == null && View3.SelectedCamera != null)
                        {
                            View2.SelectedCamera = View3.SelectedCamera;
                            View3.SelectedCamera = null;
                        }

                        FillViews();
                        break;

                    case SplitMode.Four:
                        ActiveViews = new[] { View1, View2, View3, View4 };
                        SecondColumnWidth = new GridLength(1, GridUnitType.Star);
                        SecondRowHeight = new GridLength(1, GridUnitType.Star);
                        FillViews();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                if (isLandscape ?? true)
                {
                    GeneralSettings.LandscapeSplitMode.Value = value;
                }
                else
                {
                    GeneralSettings.PortraitSplitMode.Value = value;
                }
            }
        }

        public SynchroActionSettings SynchroActions { get; } = new SynchroActionSettings();

        public string Version
        {
            get
            {
                var ver = Package.Current.Id.Version;
                return $"v{ver.Major}.{ver.Minor}.{ver.Build}";
            }
        }

        public CameraViewModel View1 => allViews[0];

        public CameraViewModel View2 => allViews[1];

        public CameraViewModel View3 => allViews[2];

        public CameraViewModel View4 => allViews[3];

        public ObservableCollection<WiFiAvailableNetwork> WifiAccessPoints { get; } = new ObservableCollection<WiFiAvailableNetwork>();

        public ObservableCollection<string> WifiAutoconnectAccessPoints { get; } = new ObservableCollection<string>();

        public bool WiFiAutoconnectAlways
        {
            get => wifi.AutoconnectAlways;
            set
            {
                wifi.AutoconnectAlways = value;
                GeneralSettings.WiFiAutoconnectAlways.Value = value;
            }
        }

        public bool WiFiPresent => wifi.Present;

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

            if (!GeneralSettings.Cameras.TryGetValue(lumix.Uuid, out var settings))
            {
                settings = new CameraSettings(lumix.Uuid);
            }

            settings.GeneralSettings = GeneralSettings;
            var connectedCamera = new ConnectedCamera
            {
                Device = lumix.Device,
                Camera = lumix,
                Model = this,
                Name = lumix.Device.FriendlyName,
                Settings = settings,
                SelectedLut = InstalledLuts.SingleOrDefault(l => l?.Id == settings.LutId),
                SelectedAspect = settings.Aspect,
                IsAspectAnamorphingVideoOnly = settings.IsAspectAnamorphingVideoOnly
            };

            ConnectedCameras.Add(connectedCamera);

            return connectedCamera;
        }

        public async Task ConnectAccessPoint(WiFiAvailableNetwork eClickedItem)
        {
            await wifi.ConnectAccessPoint(eClickedItem);
        }

        public async Task Init()
        {
            await wifi.Init();
            await LoadLutsInfo();
        }

        public async Task LoadLutsInfo()
        {
            var lutFolder = await App.GetLutsFolder();

            foreach (var file in (await lutFolder.GetFilesAsync()).Where(f => f.FileType == ".info"))
            {
                InstalledLuts.Add(await LutInfo.LoadfromFile(file));
            }
        }

        public void LumixActionCalled(Lumix camera, string method, object[] prm)
        {
            switch (Lumix.GetCommandCroup(method))
            {
                case MethodGroup.Capture:
                    if (SynchroActions.Capture)
                    {
                        RunForAll(method, prm, camera);
                    }

                    break;

                case MethodGroup.Properties:
                    if (SynchroActions.Properties)
                    {
                        RunForAll(method, prm, camera);
                    }

                    break;

                case MethodGroup.Focus:
                    if (SynchroActions.TouchAF)
                    {
                        RunForAll(method, prm, camera);
                    }

                    break;
            }
        }

        public async Task RunAsync(Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        public void ShowCamera(ConnectedCamera eClickedItem)
        {
            if (ActiveViews.Any(c => c.SelectedCamera == eClickedItem))
            {
                return;
            }

            var first = ActiveViews.Aggregate((curMin, x) => curMin == null || x.SetTime < curMin.SetTime ? x : curMin);
            if (first != null)
            {
                first.SelectedCamera = eClickedItem;
            }
        }

        public void WifiMakeCurrentAutoconnect()
        {
            WifiAutoconnectAccessPoints.Add(wifi.ConnectedWiFi);
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

        private void FillViews()
        {
            foreach (var view in ActiveViews.Where(v => v.SelectedCamera == null))
            {
                var con = ConnectedCameras.FirstOrDefault(c => ActiveViews.All(v => v.SelectedCamera != c));
                if (con != null)
                {
                    view.SelectedCamera = con;
                }
            }
        }

        private void RunForAll(string method, object[] prm, Lumix exceptCamera)
        {
            foreach (var connectedCamera in ConnectedCameras.Where(c => c.Camera != null && !ReferenceEquals(c.Camera, exceptCamera)))
            {
                Task.Run(async () => await connectedCamera.Camera.RunCommand(method, prm));
            }
        }

        private void Wifi_AccessPointsUpdated(IList<WiFiAvailableNetwork> obj)
        {
            var task = RunAsync(() =>
              {
                  WifiAccessPoints.Reset(obj);
              });
        }

        private void Wifi_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var task = RunAsync(() =>
              {
                  switch (e.PropertyName)
                  {
                      case nameof(WiFiHelper.Present):
                          OnPropertyChanged(nameof(WiFiPresent));
                          break;

                      case nameof(WiFiHelper.ConnectedWiFi):
                          OnPropertyChanged(nameof(ConnectedWiFi));
                          break;
                  }
              });
        }

        private void WifiAutoconnectAccessPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GeneralSettings.WiFiAutoconnectAccessPoints.Value = WifiAutoconnectAccessPoints;

            wifi.AutoconnectAccessPoints.Reset(WifiAutoconnectAccessPoints);
        }
    }
}