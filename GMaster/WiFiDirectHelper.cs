using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Microsoft.VisualBasic.CompilerServices;

namespace GMaster
{
    public class WiFiDirectHelper
    {
        private WiFiDirectConnectionListener _listener;
        private WiFiDirectAdvertisementPublisher _publisher;
        private DeviceWatcher _deviceWatcher;

        public WiFiDirectHelper()
        {
        }

        public WiFiDirectAdvertisementPublisherStatus Start()
        {
            _publisher = new WiFiDirectAdvertisementPublisher();
            _listener = new WiFiDirectConnectionListener();

            _publisher.StatusChanged += OnStatusChanged; ;
            try
            {
                // This can raise an exception if the machine does not support WiFi. Sorry.
                _listener.ConnectionRequested += OnConnectionRequested;
            }
            catch (Exception ex)
            {
                return WiFiDirectAdvertisementPublisherStatus.Aborted;
            }

            _publisher.Advertisement.ListenStateDiscoverability = WiFiDirectAdvertisementListenStateDiscoverability.Normal;
            _publisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;
            _publisher.Start();

            var deviceSelector = WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint);

            _deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector, new string[] { "System.Devices.WiFiDirect.InformationElements" });

            _deviceWatcher.Added += OnDeviceAdded;
            _deviceWatcher.Removed += OnDeviceRemoved;
            _deviceWatcher.Updated += OnDeviceUpdated;
            _deviceWatcher.EnumerationCompleted += OnEnumerationCompleted;
            _deviceWatcher.Stopped += OnStopped;

            _deviceWatcher.Start();

            return _publisher.Status;
        }

        private void OnStopped(DeviceWatcher sender, object args)
        {
        }

        private void OnEnumerationCompleted(DeviceWatcher sender, object args)
        {
        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }

        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
        }

        public void Stop()
        {
            _publisher.Stop();
            _publisher.StatusChanged -= OnStatusChanged;

            _listener.ConnectionRequested -= OnConnectionRequested;
        }

        private void OnStatusChanged(WiFiDirectAdvertisementPublisher sender, WiFiDirectAdvertisementPublisherStatusChangedEventArgs args)
        {
            
        }

        private void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs args)
        {
            
        }

    }
}
