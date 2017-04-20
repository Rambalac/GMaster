namespace GMaster.Views.Models
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Camera;
    using Core.Tools;
    using Windows.UI.Xaml;

    public class ConnectionsManager
    {
        private readonly LumixManager lumixManager;
        private readonly MainPageModel model;

        private readonly Network network = new Network();

        public ConnectionsManager(MainPageModel model, CultureInfo culture)
        {
            this.model = model;
            lumixManager = new LumixManager(culture.TwoLetterISOLanguageName, network);

            lumixManager.DeviceDiscovered2 += Lumix_DeviceDiscovered;

            var cameraRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            cameraRefreshTimer.Tick += CameraRefreshTimer_Tick;
            cameraRefreshTimer.Start();
        }

        public async Task ManuallyConnect(DeviceInfo dev)
        {
            await ConnectCameraFirstTime(dev);
        }

        public async Task ManuallyDisconnect(ConnectedCamera connected)
        {
            model.ConnectableDevices.Add(connected.Device);
            model.SelectedDevice = connected.Device;
            model.ConnectedCameras.Remove(connected);
            connected.MakeRemoved();
            var camera = connected.Camera;
            camera.StateUpdateFailed -= Lumix_StateUpdateFailed;
            await camera.StopStream();
            camera.Disconnect();
            camera.Dispose();
        }

        public async Task StartListening()
        {
            await lumixManager.StartListening();
            lumixManager.SearchCameras();
        }

        public void StopListening()
        {
            lumixManager.StopListening();
        }

        private void CameraRefreshTimer_Tick(object sender, object e)
        {
            lumixManager.SearchCameras();
        }

        private async Task ConnectCameraFirstTime(DeviceInfo dev)
        {
            var lumix = new Lumix(dev, new WindowsHttpClient());
            lumix.StateUpdateFailed += Lumix_StateUpdateFailed;

            var connectedCamera = model.AddConnectedDevice(lumix);
            var result = await lumixManager.ConnectCamera(lumix);
            if (result)
            {
                model.ShowCamera(connectedCamera);
                lumix.LumixState.IsBusy = false;
            }
        }

        private void Lumix_DeviceDiscovered(DeviceInfo dev, Lumix cam)
        {
            var task = model.RunAsync(() =>
              {
                  try
                  {
                      if (cam == null)
                      {
                          var existing = model.ConnectableDevices.SingleOrDefault(d => d.Uuid == dev.Uuid);
                          if (existing == null)
                          {
                              var camerafound = false;
                              var cameraauto = false;
                              if (model.GeneralSettings.Cameras.TryGetValue(dev.Uuid, out var settings))
                              {
                                  cameraauto = settings.Autoconnect;
                                  camerafound = true;
                              }

                              if ((camerafound && cameraauto) || (!camerafound && model.GeneralSettings.Autoconnect))
                              {
                                  var connecttask = model.RunAsync(async () =>
                                  {
                                      try
                                      {
                                          await ConnectCameraFirstTime(dev);
                                      }
                                      catch (Exception e)
                                      {
                                          Log.Error(e);
                                      }
                                  });
                              }
                              else
                              {
                                  model.AddConnectableDevice(dev);
                                  model.SelectedDevice = dev;
                              }
                          }
                          else
                          {
                              model.ConnectableDevices.Remove(dev);
                              model.ConnectableDevices.Add(dev);
                              model.SelectedDevice = dev;
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      Log.Error(ex);
                  }
              });
        }

        private void Lumix_StateUpdateFailed(Lumix cam, Lumix.UpdateStateFailReason reason)
        {
            cam.Disconnect();
            var task = model.RunAsync(async () =>
            {
                cam.LumixState.IsBusy = true;
                var result = await lumixManager.ConnectCamera(cam);
                if (result)
                {
                    cam.LumixState.IsBusy = false;
                }
            });
        }
    }
}